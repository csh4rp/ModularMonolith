using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ModularMonolith.Infrastructure.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        private const string TransportMigration = """
                                                  CREATE OR REPLACE FUNCTION "{0}".create_constraint_if_not_exists (t_name text, c_name text, constraint_sql text)
                                                  RETURNS void AS
                                                  $$
                                                  BEGIN
                                                      IF NOT EXISTS (SELECT constraint_name
                                                                     FROM information_schema.constraint_column_usage
                                                                     WHERE table_name = t_name AND table_schema = '{0}'
                                                                     AND constraint_name = c_name AND constraint_schema = '{0}') THEN
                                                          EXECUTE constraint_sql;
                                                      END IF;
                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE SEQUENCE IF NOT EXISTS "{0}".topology_seq AS bigint;

                                                  CREATE TABLE IF NOT EXISTS "{0}".queue
                                                  (
                                                      id          bigint          not null primary key default nextval('"{0}".topology_seq'),
                                                      updated     timestamptz     not null default (now() at time zone 'utc'),

                                                      name        text            not null,
                                                      type        integer         not null,
                                                      auto_delete integer
                                                  );

                                                  SELECT "{0}".create_constraint_if_not_exists('queue', 'unique_queue',
                                                          'CREATE UNIQUE INDEX IF NOT EXISTS queue_uqx ON "{0}".queue (type, name) INCLUDE (id);ALTER TABLE "{0}".queue ADD CONSTRAINT unique_queue UNIQUE USING INDEX queue_uqx;');

                                                  CREATE INDEX IF NOT EXISTS queue_auto_delete_ndx ON "{0}".queue (auto_delete) INCLUDE (id);

                                                  CREATE TABLE IF NOT EXISTS "{0}".topic
                                                  (
                                                      id          bigint      not null primary key default nextval('"{0}".topology_seq'),
                                                      updated     timestamptz not null default (now() at time zone 'utc'),

                                                      name        text        not null
                                                  );

                                                  SELECT "{0}".create_constraint_if_not_exists('topic', 'unique_topic',
                                                          'CREATE UNIQUE INDEX IF NOT EXISTS topic_uqx ON "{0}".topic (name) INCLUDE (id);ALTER TABLE "{0}".topic ADD CONSTRAINT unique_topic UNIQUE USING INDEX topic_uqx;');

                                                  CREATE TABLE IF NOT EXISTS "{0}".topic_subscription
                                                  (
                                                      id              bigint       not null primary key default nextval('"{0}".topology_seq'),
                                                      updated         timestamptz  not null default (now() at time zone 'utc'),

                                                      source_id       bigint       not null references "{0}".topic (id) ON DELETE CASCADE,
                                                      destination_id  bigint       not null references "{0}".topic (id) ON DELETE CASCADE,

                                                      sub_type        integer      not null,
                                                      routing_key     text         not null,
                                                      filter          jsonb        not null
                                                  );

                                                  SELECT "{0}".create_constraint_if_not_exists('topic_subscription', 'unique_topic_subscription',
                                                          'CREATE UNIQUE INDEX IF NOT EXISTS topic_subscription_uqx ON "{0}".topic_subscription (source_id, destination_id, sub_type, routing_key, filter) INCLUDE (id);ALTER TABLE "{0}".topic_subscription ADD CONSTRAINT unique_topic_subscription UNIQUE USING INDEX topic_subscription_uqx;');

                                                  CREATE INDEX IF NOT EXISTS topic_subscription_source_id_ndx ON "{0}".topic_subscription (source_id) INCLUDE (id, destination_id);

                                                  CREATE INDEX IF NOT EXISTS topic_subscription_destination_id_ndx ON "{0}".topic_subscription (destination_id) INCLUDE (id, source_id);

                                                  CREATE TABLE IF NOT EXISTS "{0}".queue_subscription
                                                  (
                                                      id              bigint       not null primary key default nextval('"{0}".topology_seq'),
                                                      updated         timestamptz  not null default (now() at time zone 'utc'),

                                                      source_id       bigint       not null references "{0}".topic (id) ON DELETE CASCADE,
                                                      destination_id  bigint       not null references "{0}".queue (id) ON DELETE CASCADE,

                                                      sub_type        integer      not null,
                                                      routing_key     text         not null,
                                                      filter          jsonb        not null
                                                  );

                                                  SELECT "{0}".create_constraint_if_not_exists('queue_subscription', 'unique_queue_subscription',
                                                          'CREATE UNIQUE INDEX IF NOT EXISTS queue_subscription_uqx ON "{0}".queue_subscription (source_id, destination_id, sub_type, routing_key, filter);ALTER TABLE "{0}".queue_subscription ADD CONSTRAINT unique_queue_subscription UNIQUE USING INDEX queue_subscription_uqx;');

                                                  CREATE INDEX IF NOT EXISTS queue_subscription_source_id_ndx ON "{0}".queue_subscription (source_id) INCLUDE (id, destination_id);

                                                  CREATE INDEX IF NOT EXISTS queue_subscription_destination_id_ndx ON "{0}".queue_subscription (destination_id) INCLUDE (id, source_id);

                                                  CREATE TABLE IF NOT EXISTS "{0}".message
                                                  (
                                                      transport_message_id uuid        not null primary key,

                                                      content_type         text,
                                                      message_type         text,
                                                      body                 jsonb,
                                                      binary_body          bytea,

                                                      message_id           uuid,
                                                      correlation_id       uuid,
                                                      conversation_id      uuid,
                                                      request_id           uuid,
                                                      initiator_id         uuid,
                                                      scheduling_token_id  uuid,

                                                      source_address       text,
                                                      destination_address  text,
                                                      response_address     text,
                                                      fault_address        text,

                                                      sent_time            timestamptz NOT NULL DEFAULT (now() at time zone 'utc'),

                                                      headers              jsonb,
                                                      host                 jsonb
                                                  );

                                                  CREATE INDEX IF NOT EXISTS message_scheduling_token_id_ndx ON "{0}".message (scheduling_token_id) where message.scheduling_token_id IS NOT NULL;

                                                  CREATE TABLE IF NOT EXISTS "{0}".message_delivery
                                                  (
                                                      message_delivery_id     bigserial   not null primary key,
                                                      transport_message_id    uuid        not null REFERENCES "{0}".message ON DELETE CASCADE,

                                                      queue_id                bigint      not null,
                                                      priority                smallint    not null,
                                                      enqueue_time            timestamptz not null,
                                                      expiration_time         timestamptz,

                                                      partition_key           text,
                                                      routing_key             text,

                                                      consumer_id             uuid,
                                                      lock_id                 uuid,

                                                      delivery_count          int         not null,
                                                      max_delivery_count      int         not null,
                                                      last_delivered          timestamptz,
                                                      transport_headers       jsonb
                                                  );

                                                  CREATE INDEX IF NOT EXISTS message_delivery_fetch_ndx on "{0}".message_delivery (queue_id, priority, enqueue_time, message_delivery_id);

                                                  CREATE INDEX IF NOT EXISTS message_delivery_fetch_part_ndx on "{0}".message_delivery (queue_id, partition_key, priority, enqueue_time, message_delivery_id);

                                                  CREATE INDEX IF NOT EXISTS message_delivery_transport_message_id_ndx ON "{0}".message_delivery (transport_message_id);

                                                  CREATE OR REPLACE FUNCTION "{0}".create_queue(queue_name text, auto_delete integer DEFAULT NULL)
                                                      RETURNS integer
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_queue_id bigint;
                                                  BEGIN
                                                      IF queue_name IS NULL OR LENGTH(queue_name) < 1 THEN
                                                          RAISE EXCEPTION 'Queue names must not be null or empty';
                                                      END IF;

                                                      INSERT INTO "{0}".queue (name, type, auto_delete) VALUES (queue_name, 1, auto_delete)
                                                          ON CONFLICT ON CONSTRAINT unique_queue DO
                                                          UPDATE SET updated = (now() at time zone 'utc'), auto_delete = COALESCE(create_queue.auto_delete, excluded.auto_delete)
                                                          RETURNING queue.id INTO v_queue_id;

                                                      INSERT INTO "{0}".queue (name, type, auto_delete) VALUES (queue_name, 2, auto_delete)
                                                          ON CONFLICT ON CONSTRAINT unique_queue DO
                                                          UPDATE SET updated = (now() at time zone 'utc'), auto_delete = COALESCE(create_queue.auto_delete, excluded.auto_delete);

                                                      INSERT INTO "{0}".queue (name, type, auto_delete) VALUES (queue_name, 3, auto_delete)
                                                          ON CONFLICT ON CONSTRAINT unique_queue DO
                                                          UPDATE SET updated = (now() at time zone 'utc'), auto_delete = COALESCE(create_queue.auto_delete, excluded.auto_delete);

                                                      RETURN v_queue_id;

                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE OR REPLACE FUNCTION "{0}".create_topic(topic_name text)
                                                      RETURNS integer
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_topic_id bigint;
                                                  BEGIN
                                                      IF topic_name IS NULL OR LENGTH(topic_name) < 1 THEN
                                                          RAISE EXCEPTION 'Topic names must not be null or empty';
                                                      END IF;

                                                      INSERT INTO "{0}".topic (name) VALUES (topic_name)
                                                          ON CONFLICT ON CONSTRAINT unique_topic DO
                                                          UPDATE SET updated = (now() at time zone 'utc')
                                                          RETURNING topic.id INTO v_topic_id;

                                                      RETURN v_topic_id;

                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE OR REPLACE FUNCTION "{0}".create_topic_subscription(source_topic_name text, destination_topic_name text, type integer,
                                                      routing_key text DEFAULT '', filter jsonb DEFAULT '{{}}')
                                                      RETURNS integer
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_topic_subscription_id bigint;
                                                      v_source_id bigint;
                                                      v_destination_id bigint;
                                                  BEGIN
                                                      IF source_topic_name IS NULL OR LENGTH(source_topic_name) < 1 THEN
                                                          RAISE EXCEPTION 'Topic names must not be null or empty';
                                                      END IF;
                                                      IF destination_topic_name IS NULL OR LENGTH(destination_topic_name) < 1 THEN
                                                          RAISE EXCEPTION 'Topic names must not be null or empty';
                                                      END IF;

                                                      SELECT INTO v_source_id t.Id FROM "{0}".topic t WHERE t.name = source_topic_name;
                                                      IF v_source_id IS NULL THEN
                                                          RAISE EXCEPTION 'Source topic not found: %', source_topic_name;
                                                      END IF;
                                                      SELECT INTO v_destination_id t.Id FROM "{0}".topic t WHERE t.name = destination_topic_name;
                                                      IF v_destination_id IS NULL THEN
                                                          RAISE EXCEPTION 'Destination topic not found: %', destination_topic_name;
                                                      END IF;

                                                      INSERT INTO "{0}".topic_subscription (source_id, destination_id, sub_type, routing_key, filter)
                                                          VALUES (v_source_id, v_destination_id, type, COALESCE(create_topic_subscription.routing_key, ''), COALESCE(create_topic_subscription.filter, '{{}}'::jsonb))
                                                          ON CONFLICT ON CONSTRAINT unique_topic_subscription DO
                                                          UPDATE SET updated = (now() at time zone 'utc')
                                                          RETURNING topic_subscription.id INTO v_topic_subscription_id;

                                                      RETURN v_topic_subscription_id;

                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE OR REPLACE FUNCTION "{0}".create_queue_subscription(source_topic_name text, destination_queue_name text, type integer,
                                                      routing_key text DEFAULT '', filter jsonb DEFAULT '{{}}')
                                                      RETURNS integer
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_queue_subscription_id bigint;
                                                      v_source_id bigint;
                                                      v_destination_id bigint;
                                                  BEGIN
                                                      IF source_topic_name IS NULL OR LENGTH(source_topic_name) < 1 THEN
                                                          RAISE EXCEPTION 'Topic names must not be null or empty';
                                                      END IF;
                                                      IF destination_queue_name IS NULL OR LENGTH(destination_queue_name) < 1 THEN
                                                          RAISE EXCEPTION 'Queue names must not be null or empty';
                                                      END IF;

                                                      SELECT INTO v_source_id t.Id FROM "{0}".topic t WHERE t.name = source_topic_name;
                                                      IF v_source_id IS NULL THEN
                                                          RAISE EXCEPTION 'Source topic not found: %', source_topic_name;
                                                      END IF;
                                                      SELECT INTO v_destination_id q.Id FROM "{0}".queue q WHERE q.name = destination_queue_name AND q.type = 1;
                                                      IF v_destination_id IS NULL THEN
                                                          RAISE EXCEPTION 'Destination queue not found: %', destination_queue_name;
                                                      END IF;

                                                      INSERT INTO "{0}".queue_subscription (source_id, destination_id, sub_type, routing_key, filter)
                                                          VALUES (v_source_id, v_destination_id, type, COALESCE(create_queue_subscription.routing_key, ''), COALESCE(create_queue_subscription.filter, '{{}}'::jsonb))
                                                          ON CONFLICT ON CONSTRAINT unique_queue_subscription DO
                                                          UPDATE SET updated = (now() at time zone 'utc')
                                                          RETURNING queue_subscription.id INTO v_queue_subscription_id;

                                                      RETURN v_queue_subscription_id;

                                                  END;
                                                  $$ LANGUAGE plpgsql;


                                                  CREATE OR REPLACE FUNCTION "{0}".purge_queue(queue_name text)
                                                      RETURNS bigint
                                                  AS
                                                  $$
                                                  BEGIN
                                                      IF queue_name IS NULL OR LENGTH(queue_name) < 1 THEN
                                                          RAISE EXCEPTION 'Queue name must not be null';
                                                      END IF;

                                                      WITH msgs AS (
                                                          DELETE FROM "{0}".message_delivery md
                                                              USING (SELECT mdx.message_delivery_id
                                                                     FROM "{0}".message_delivery mdx
                                                                         INNER JOIN "{0}".queue q on mdx.queue_id = q.Id
                                                                     WHERE q.name = queue_name) mds
                                                              WHERE md.message_delivery_id = mds.message_delivery_id
                                                              RETURNING md.transport_message_id)
                                                          DELETE FROM "{0}".message m
                                                              USING msgs
                                                              WHERE m.transport_message_id = msgs.transport_message_id
                                                                  AND NOT EXISTS(SELECT FROM "{0}".message_delivery md WHERE md.transport_message_id = m.transport_message_id);

                                                      RETURN 0;
                                                  END;
                                                  $$ LANGUAGE plpgsql;


                                                  CREATE OR REPLACE FUNCTION "{0}".fetch_messages(
                                                      queue_name text
                                                  ,   fetch_consumer_id uuid
                                                  ,   fetch_lock_id uuid
                                                  ,   lock_duration interval
                                                  ,   fetch_count integer DEFAULT 1)
                                                      RETURNS TABLE(
                                                      transport_message_id uuid
                                                  ,   queue_id bigint
                                                  ,   priority smallint
                                                  ,   message_delivery_id bigint
                                                  ,   consumer_id uuid
                                                  ,   lock_id uuid
                                                  ,   enqueue_time timestamp with time zone
                                                  ,   expiration_time timestamp with time zone
                                                  ,   delivery_count integer
                                                  ,   partition_key text
                                                  ,   routing_key text
                                                  ,   transport_headers jsonb
                                                  ,   content_type text
                                                  ,   message_type text
                                                  ,   body jsonb
                                                  ,   binary_body bytea
                                                  ,   message_id uuid
                                                  ,   correlation_id uuid
                                                  ,   conversation_id uuid
                                                  ,   request_id uuid
                                                  ,   initiator_id uuid
                                                  ,   source_address text
                                                  ,   destination_address text
                                                  ,   response_address text
                                                  ,   fault_address text
                                                  ,   sent_time timestamp with time zone
                                                  ,   headers jsonb
                                                  ,   host jsonb)
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_queue_id bigint;
                                                      v_enqueue_time timestamptz;
                                                      v_now timestamptz;
                                                  BEGIN
                                                      SELECT INTO v_queue_id q.Id FROM "{0}".queue q WHERE q.name = queue_name AND q.type = 1;
                                                      IF v_queue_id IS NULL THEN
                                                          RAISE EXCEPTION 'Queue not found: %', queue_name;
                                                      END IF;

                                                      v_now := (now() at time zone 'utc');
                                                      v_enqueue_time := v_now + lock_duration;

                                                      RETURN QUERY WITH msgs AS (
                                                          SELECT md.*
                                                          FROM "{0}".message_delivery md
                                                          WHERE md.queue_id = v_queue_id
                                                              AND md.enqueue_time <= v_now
                                                              AND md.delivery_count < md.max_delivery_count
                                                          ORDER BY md.priority, md.enqueue_time, md.message_delivery_id
                                                          LIMIT fetch_count FOR UPDATE OF md SKIP LOCKED)
                                                          UPDATE "{0}".message_delivery dm
                                                          SET delivery_count = dm.delivery_count + 1,
                                                              last_delivered = v_now,
                                                              consumer_id = fetch_consumer_id,
                                                              lock_id = fetch_lock_id,
                                                              enqueue_time = v_enqueue_time
                                                              FROM msgs
                                                              INNER JOIN "{0}".message m on msgs.transport_message_id = m.transport_message_id
                                                          WHERE dm.message_delivery_id = msgs.message_delivery_id
                                                          RETURNING
                                                              dm.transport_message_id,
                                                              dm.queue_id,
                                                              dm.priority,
                                                              dm.message_delivery_id,
                                                              dm.consumer_id,
                                                              dm.lock_id,
                                                              dm.enqueue_time,
                                                              dm.expiration_time,
                                                              dm.delivery_count,
                                                              dm.partition_key,
                                                              dm.routing_key,
                                                              dm.transport_headers,
                                                              m.content_type,
                                                              m.message_type,
                                                              m.body,
                                                              m.binary_body,
                                                              m.message_id,
                                                              m.correlation_id,
                                                              m.conversation_id,
                                                              m.request_id,
                                                              m.initiator_id,
                                                              m.source_address,
                                                              m.destination_address,
                                                              m.response_address,
                                                              m.fault_address,
                                                              m.sent_time,
                                                              m.headers,
                                                              m.host;
                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE OR REPLACE FUNCTION "{0}".fetch_messages_partitioned(
                                                      queue_name text
                                                  ,   fetch_consumer_id uuid
                                                  ,   fetch_lock_id uuid
                                                  ,   lock_duration interval
                                                  ,   fetch_count integer DEFAULT 1
                                                  ,   concurrent_count integer DEFAULT 1
                                                  ,   ordered integer DEFAULT 0)
                                                      RETURNS TABLE(
                                                      transport_message_id uuid
                                                  ,   queue_id bigint
                                                  ,   priority smallint
                                                  ,   message_delivery_id bigint
                                                  ,   consumer_id uuid
                                                  ,   lock_id uuid
                                                  ,   enqueue_time timestamp with time zone
                                                  ,   expiration_time timestamp with time zone
                                                  ,   delivery_count integer
                                                  ,   partition_key text
                                                  ,   routing_key text
                                                  ,   transport_headers jsonb
                                                  ,   content_type text
                                                  ,   message_type text
                                                  ,   body jsonb
                                                  ,   binary_body bytea
                                                  ,   message_id uuid
                                                  ,   correlation_id uuid
                                                  ,   conversation_id uuid
                                                  ,   request_id uuid
                                                  ,   initiator_id uuid
                                                  ,   source_address text
                                                  ,   destination_address text
                                                  ,   response_address text
                                                  ,   fault_address text
                                                  ,   sent_time timestamp with time zone
                                                  ,   headers jsonb
                                                  ,   host jsonb)
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_queue_id bigint;
                                                      v_enqueue_time timestamptz;
                                                      v_now timestamptz;
                                                  BEGIN
                                                      SELECT INTO v_queue_id q.Id FROM "{0}".queue q WHERE q.name = queue_name AND q.type = 1;
                                                      IF v_queue_id IS NULL THEN
                                                          RAISE EXCEPTION 'Queue not found: %', queue_name;
                                                      END IF;

                                                      v_now := (now() at time zone 'utc');
                                                      v_enqueue_time := v_now + lock_duration;

                                                      RETURN QUERY WITH msgs AS (
                                                          SELECT md.*
                                                          FROM "{0}".message_delivery md
                                                          WHERE md.message_delivery_id IN (
                                                              WITH ready AS (
                                                                  SELECT mdx.message_delivery_id, mdx.enqueue_time, mdx.lock_id, mdx.priority,
                                                                          row_number() over ( partition by mdx.partition_key
                                                                              order by mdx.priority, mdx.enqueue_time, mdx.message_delivery_id ) as row_normal,
                                                                          row_number() over ( partition by mdx.partition_key
                                                                              order by mdx.priority, mdx.message_delivery_id,mdx.enqueue_time )  as row_ordered,
                                                                         first_value(CASE WHEN mdx.enqueue_time > v_now THEN mdx.consumer_id END) over (partition by mdx.partition_key
                                                                             order by mdx.enqueue_time DESC, mdx.message_delivery_id DESC) as consumer_id
                                                                  FROM "{0}".message_delivery mdx
                                                                  WHERE mdx.queue_id = v_queue_id
                                                                      AND mdx.delivery_count < mdx.max_delivery_count
                                                              )
                                                              SELECT ready.message_delivery_id
                                                                  FROM ready
                                                                  WHERE ( ( ordered = 0 AND ready.row_normal <= concurrent_count) OR ( ordered = 1 AND ready.row_ordered <= concurrent_count ) )
                                                                  AND ready.enqueue_time <= v_now
                                                                  AND (ready.consumer_id IS NULL OR ready.consumer_id = fetch_consumer_id)
                                                                  ORDER BY ready.priority, ready.enqueue_time, ready.message_delivery_id
                                                              LIMIT fetch_count FOR UPDATE SKIP LOCKED)
                                                          FOR UPDATE OF md SKIP LOCKED)
                                                          UPDATE "{0}".message_delivery dm
                                                          SET delivery_count = dm.delivery_count + 1,
                                                              last_delivered = v_now,
                                                              consumer_id = fetch_consumer_id,
                                                              lock_id = fetch_lock_id,
                                                              enqueue_time = v_enqueue_time
                                                              FROM msgs
                                                              INNER JOIN "{0}".message m on msgs.transport_message_id = m.transport_message_id
                                                          WHERE dm.message_delivery_id = msgs.message_delivery_id
                                                          RETURNING
                                                              dm.transport_message_id,
                                                              dm.queue_id,
                                                              dm.priority,
                                                              dm.message_delivery_id,
                                                              dm.consumer_id,
                                                              dm.lock_id,
                                                              dm.enqueue_time,
                                                              dm.expiration_time,
                                                              dm.delivery_count,
                                                              dm.partition_key,
                                                              dm.routing_key,
                                                              dm.transport_headers,
                                                              m.content_type,
                                                              m.message_type,
                                                              m.body,
                                                              m.binary_body,
                                                              m.message_id,
                                                              m.correlation_id,
                                                              m.conversation_id,
                                                              m.request_id,
                                                              m.initiator_id,
                                                              m.source_address,
                                                              m.destination_address,
                                                              m.response_address,
                                                              m.fault_address,
                                                              m.sent_time,
                                                              m.headers,
                                                              m.host;
                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE OR REPLACE FUNCTION "{0}".delete_message(message_delivery_id bigint, lock_id uuid)
                                                      RETURNS bigint
                                                      LANGUAGE PLPGSQL
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_message_delivery_id   bigint;
                                                      v_queue_id              bigint;
                                                      v_transport_message_id  uuid;
                                                  BEGIN
                                                      DELETE FROM "{0}".message_delivery md
                                                          WHERE md.message_delivery_id = delete_message.message_delivery_id
                                                          AND md.lock_id = delete_message.lock_id
                                                          RETURNING md.message_delivery_id, md.transport_message_id, md.queue_id INTO v_message_delivery_id, v_transport_message_id, v_queue_id;

                                                      IF v_transport_message_id IS NOT NULL THEN
                                                          DELETE FROM "{0}".message m
                                                              WHERE m.transport_message_id = v_transport_message_id
                                                              AND NOT EXISTS(SELECT FROM "{0}".message_delivery md WHERE md.transport_message_id = v_transport_message_id);

                                                          INSERT INTO "{0}".queue_metric_capture (captured, queue_id, consume_count, error_count, dead_letter_count)
                                                              VALUES (now() at time zone 'utc', v_queue_id, 1, 0, 0);

                                                      END IF;

                                                      RETURN v_message_delivery_id;
                                                  END;
                                                  $$;

                                                  CREATE OR REPLACE FUNCTION "{0}".delete_scheduled_message(token_id uuid)
                                                      RETURNS TABLE (transport_message_id uuid)
                                                      LANGUAGE PLPGSQL
                                                  AS
                                                  $$
                                                  BEGIN
                                                      RETURN QUERY DELETE FROM "{0}".message tm
                                                          USING "{0}".message as m
                                                          LEFT JOIN "{0}".message_delivery md ON md.transport_message_id = m.transport_message_id
                                                      WHERE tm.transport_message_id = m.transport_message_id
                                                          AND m.scheduling_token_id = token_id
                                                          AND md.delivery_count = 0
                                                          AND md.lock_id IS NULL
                                                      RETURNING tm.transport_message_id;
                                                  END;
                                                  $$;

                                                  CREATE OR REPLACE FUNCTION "{0}".renew_message_lock(message_delivery_id bigint, lock_id uuid, duration interval)
                                                      RETURNS bigint
                                                      LANGUAGE PLPGSQL
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_message_delivery_id   bigint;
                                                      v_queue_id              bigint;
                                                  BEGIN
                                                      IF duration < INTERVAL '1 seconds' THEN
                                                          RAISE EXCEPTION 'Invalid lock duration';
                                                      END IF;

                                                      UPDATE "{0}".message_delivery md
                                                          SET enqueue_time = (now() at time zone 'utc') + duration
                                                          WHERE md.message_delivery_id = renew_message_lock.message_delivery_id AND md.lock_id = renew_message_lock.lock_id
                                                          RETURNING md.message_delivery_id, md.queue_id INTO v_message_delivery_id, v_queue_id;

                                                      IF v_queue_id IS NOT NULL THEN
                                                          INSERT INTO "{0}".queue_metric_capture (captured, queue_id, consume_count, error_count, dead_letter_count)
                                                              VALUES (now() at time zone 'utc', v_queue_id, 0, 0, 0);
                                                      END IF;

                                                      RETURN v_message_delivery_id;
                                                  END;
                                                  $$;

                                                  CREATE OR REPLACE FUNCTION "{0}".move_message(message_delivery_id bigint, lock_id uuid, queue_name text, queue_type integer, headers jsonb)
                                                      RETURNS bigint
                                                      LANGUAGE PLPGSQL
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_message_delivery_id   bigint;
                                                      v_queue_id              bigint;
                                                      v_source_queue_id       bigint;
                                                      v_enqueue_time          timestamptz;
                                                  BEGIN
                                                      SELECT INTO v_queue_id q.Id FROM "{0}".queue q WHERE q.name = queue_name AND q.type = queue_type;
                                                      IF v_queue_id IS NULL THEN
                                                          RAISE EXCEPTION 'Queue not found: %', queue_name;
                                                      END IF;

                                                      v_enqueue_time := (now() at time zone 'utc');

                                                      UPDATE "{0}".message_delivery md
                                                          SET enqueue_time = v_enqueue_time, queue_id = v_queue_id, lock_id = NULL, consumer_id = NULL, transport_headers = headers
                                                          FROM (SELECT mdx.message_delivery_id, queue_id, consumer_id FROM "{0}".message_delivery mdx
                                                              WHERE mdx.message_delivery_id = move_message.message_delivery_id AND mdx.lock_id = move_message.lock_id FOR UPDATE) mdy
                                                          WHERE mdy.message_delivery_id = md.message_delivery_id
                                                          RETURNING md.message_delivery_id, mdy.queue_id INTO v_message_delivery_id, v_source_queue_id;

                                                      IF v_source_queue_id IS NOT NULL THEN
                                                          INSERT INTO "{0}".queue_metric_capture (captured, queue_id, consume_count, error_count, dead_letter_count)
                                                              VALUES (now() at time zone 'utc', v_source_queue_id, 0,
                                                              CASE WHEN queue_type = 2 THEN 1 ELSE 0 END, CASE WHEN queue_type = 3 THEN 1 ELSE 0 END);
                                                      END IF;

                                                      RETURN v_message_delivery_id;
                                                  END;
                                                  $$;

                                                  CREATE OR REPLACE FUNCTION "{0}".unlock_message(message_delivery_id bigint, lock_id uuid, delay interval, headers jsonb)
                                                      RETURNS bigint
                                                      LANGUAGE PLPGSQL
                                                  AS
                                                  $$
                                                  DECLARE
                                                      v_message_delivery_id   bigint;
                                                      v_enqueue_time          timestamptz;
                                                      v_queue_id              bigint;
                                                  BEGIN
                                                      v_enqueue_time := (now() at time zone 'utc');
                                                      IF delay > INTERVAL '0 seconds' THEN
                                                          v_enqueue_time = v_enqueue_time + delay;
                                                      END IF;

                                                      UPDATE "{0}".message_delivery md
                                                          SET enqueue_time = v_enqueue_time, consumer_id = NULL, transport_headers = headers
                                                          WHERE md.message_delivery_id = unlock_message.message_delivery_id AND md.lock_id = unlock_message.lock_id
                                                          RETURNING md.message_delivery_id, md.queue_id INTO v_message_delivery_id, v_queue_id;

                                                      IF v_queue_id IS NOT NULL THEN
                                                          INSERT INTO "{0}".queue_metric_capture (captured, queue_id, consume_count, error_count, dead_letter_count)
                                                              VALUES (now() at time zone 'utc', v_queue_id, 0, 0, 0);
                                                      END IF;

                                                      RETURN v_message_delivery_id;
                                                  END;
                                                  $$;

                                                  CREATE OR REPLACE FUNCTION "{0}".send_message(
                                                    entity_name text
                                                  , priority integer DEFAULT NULL
                                                  , transport_message_id uuid DEFAULT gen_random_uuid()
                                                  , body jsonb DEFAULT NULL
                                                  , binary_body bytea DEFAULT NULL
                                                  , content_type text DEFAULT NULL
                                                  , message_type text DEFAULT NULL
                                                  , message_id uuid DEFAULT NULL
                                                  , correlation_id uuid DEFAULT NULL
                                                  , conversation_id uuid DEFAULT NULL
                                                  , request_id uuid DEFAULT NULL
                                                  , initiator_id uuid DEFAULT NULL
                                                  , source_address text DEFAULT NULL
                                                  , destination_address text DEFAULT NULL
                                                  , response_address text DEFAULT NULL
                                                  , fault_address text DEFAULT NULL
                                                  , sent_time timestamptz DEFAULT NULL
                                                  , headers jsonb DEFAULT NULL
                                                  , host jsonb DEFAULT NULL
                                                  , partition_key text DEFAULT NULL
                                                  , routing_key text DEFAULT NULL
                                                  , delay interval DEFAULT INTERVAL '0 seconds'
                                                  , scheduling_token_id uuid DEFAULT NULL
                                                  , max_delivery_count int DEFAULT 10
                                                  )
                                                      RETURNS bigint AS
                                                  $$
                                                  DECLARE
                                                      v_queue_id     bigint;
                                                      v_enqueue_time timestamptz;
                                                  BEGIN
                                                      if entity_name is null or length(entity_name) < 1 then
                                                          raise exception 'Queue names must not be null or empty';
                                                      end if;

                                                      SELECT INTO v_queue_id q.Id FROM "{0}".queue q WHERE q.name = entity_name AND q.type = 1;
                                                      if v_queue_id IS NULL THEN
                                                          raise exception 'Queue not found';
                                                      end if;

                                                      v_enqueue_time := (now() at time zone 'utc');
                                                      IF delay > INTERVAL '0 seconds' THEN
                                                          v_enqueue_time = v_enqueue_time + delay;
                                                      END IF;

                                                      INSERT INTO "{0}".message (transport_message_id, body, binary_body, content_type, message_type, message_id, correlation_id, conversation_id, request_id, initiator_id,
                                                          source_address, destination_address, response_address, fault_address, sent_time, headers, host, scheduling_token_id)
                                                      VALUES (transport_message_id, body, binary_body, content_type, message_type, message_id, correlation_id, conversation_id, request_id, initiator_id,
                                                          source_address, destination_address, response_address, fault_address, sent_time, headers, host, scheduling_token_id);
                                                      INSERT INTO "{0}".message_delivery (queue_id, transport_message_id, priority, enqueue_time, delivery_count, max_delivery_count, partition_key, routing_key)
                                                      VALUES (v_queue_id, send_message.transport_message_id, send_message.priority, v_enqueue_time, 0, send_message.max_delivery_count, send_message.partition_key, send_message.routing_key);

                                                      RETURN 1;

                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE OR REPLACE FUNCTION "{0}".publish_message(
                                                    entity_name text
                                                  , priority integer DEFAULT NULL
                                                  , transport_message_id uuid DEFAULT gen_random_uuid()
                                                  , body jsonb DEFAULT NULL
                                                  , binary_body bytea DEFAULT NULL
                                                  , content_type text DEFAULT NULL
                                                  , message_type text DEFAULT NULL
                                                  , message_id uuid DEFAULT NULL
                                                  , correlation_id uuid DEFAULT NULL
                                                  , conversation_id uuid DEFAULT NULL
                                                  , request_id uuid DEFAULT NULL
                                                  , initiator_id uuid DEFAULT NULL
                                                  , source_address text DEFAULT NULL
                                                  , destination_address text DEFAULT NULL
                                                  , response_address text DEFAULT NULL
                                                  , fault_address text DEFAULT NULL
                                                  , sent_time timestamptz DEFAULT NULL
                                                  , headers jsonb DEFAULT NULL
                                                  , host jsonb DEFAULT NULL
                                                  , partition_key text DEFAULT NULL
                                                  , routing_key text DEFAULT NULL
                                                  , delay interval DEFAULT INTERVAL '0 seconds'
                                                  , scheduling_token_id uuid DEFAULT NULL
                                                  , max_delivery_count int DEFAULT 10
                                                  )
                                                      RETURNS bigint AS
                                                  $$
                                                  DECLARE
                                                      v_topic_id      bigint;
                                                      v_enqueue_time  timestamptz;
                                                      v_publish_count bigint;
                                                  BEGIN
                                                      IF entity_name IS NULL OR LENGTH(entity_name) < 1 THEN
                                                          RAISE EXCEPTION 'Topic names must not be null or empty';
                                                      END IF;

                                                      SELECT INTO v_topic_id t.Id FROM "{0}".topic t WHERE t.name = entity_name;
                                                      if v_topic_id IS NULL THEN
                                                          RAISE EXCEPTION 'Topic not found';
                                                      END IF;

                                                      v_enqueue_time := (now() at time zone 'utc');
                                                      IF delay > INTERVAL '0 seconds' THEN
                                                          v_enqueue_time = v_enqueue_time + delay;
                                                      END IF;

                                                      INSERT INTO "{0}".message (transport_message_id, body, binary_body, content_type, message_type, message_id, correlation_id, conversation_id, request_id, initiator_id,
                                                          source_address, destination_address, response_address, fault_address, sent_time, headers, host, scheduling_token_id)
                                                      VALUES (transport_message_id, body, binary_body, content_type, message_type, message_id, correlation_id, conversation_id, request_id, initiator_id,
                                                          source_address, destination_address, response_address, fault_address, sent_time, headers, host, scheduling_token_id);

                                                      WITH delivered AS (
                                                      INSERT INTO "{0}".message_delivery (queue_id, transport_message_id, priority, enqueue_time, delivery_count, max_delivery_count, partition_key, routing_key)
                                                      WITH RECURSIVE fabric AS (
                                                          SELECT source_id, destination_id
                                                              FROM "{0}".topic t
                                                              LEFT JOIN "{0}".topic_subscription ts ON t.id = ts.source_id
                                                              AND CASE
                                                                  WHEN ts.sub_type = 1 THEN true
                                                                  WHEN ts.sub_type = 2 THEN publish_message.routing_key = ts.routing_key
                                                                  WHEN ts.sub_type = 3 THEN publish_message.routing_key ~ ts.routing_key
                                                                  ELSE false END
                                                              WHERE t.id = v_topic_id

                                                        UNION ALL

                                                          SELECT ts.source_id, ts.destination_id
                                                              FROM "{0}".topic_subscription ts, fabric
                                                              WHERE ts.source_id = fabric.destination_id
                                                              AND CASE
                                                                  WHEN ts.sub_type = 1 THEN true
                                                                  WHEN ts.sub_type = 2 THEN publish_message.routing_key = ts.routing_key
                                                                  WHEN ts.sub_type = 3 THEN publish_message.routing_key ~ ts.routing_key
                                                                  ELSE false END
                                                          )
                                                      SELECT DISTINCT qs.destination_id, publish_message.transport_message_id, publish_message.priority, v_enqueue_time, 0, publish_message.max_delivery_count, publish_message.partition_key, publish_message.routing_key
                                                          FROM "{0}".queue_subscription qs, fabric
                                                          WHERE CASE
                                                              WHEN qs.sub_type = 1 THEN true
                                                              WHEN qs.sub_type = 2 THEN publish_message.routing_key = qs.routing_key
                                                              WHEN qs.sub_type = 3 THEN publish_message.routing_key ~ qs.routing_key
                                                              ELSE false END
                                                          AND (qs.source_id = fabric.destination_id OR qs.source_id = v_topic_id)
                                                      RETURNING message_delivery_id)
                                                      SELECT COUNT(d.message_delivery_id) FROM delivered d INTO v_publish_count;

                                                      IF v_publish_count = 0 THEN
                                                          DELETE FROM "{0}".message WHERE message.transport_message_id = publish_message.transport_message_id;
                                                      END IF;

                                                      RETURN v_publish_count;

                                                  END;
                                                  $$ LANGUAGE plpgsql;

                                                  CREATE OR REPLACE FUNCTION "{0}".notify_msg()
                                                      RETURNS trigger AS
                                                  $$
                                                  DECLARE
                                                      v_payload   json;
                                                  BEGIN
                                                      IF NEW.enqueue_time <= (now() at time zone 'utc') THEN
                                                          v_payload = json_build_object(
                                                              'message_delivery_id', NEW.message_delivery_id,
                                                              'enqueue_time', to_char(NEW.enqueue_time, 'YYYY-MM-DD"T"HH24:MI:SS.MS"Z"')
                                                          );

                                                          PERFORM pg_notify('{0}_msg_' || NEW.queue_id, v_payload::text);
                                                      END IF;

                                                      RETURN NEW;
                                                  END;
                                                  $$ LANGUAGE plpgsql VOLATILE;

                                                  CREATE OR REPLACE TRIGGER message_delivery_notify_trigger AFTER INSERT OR UPDATE ON "{0}".message_delivery
                                                      FOR EACH ROW EXECUTE PROCEDURE "{0}".notify_msg();

                                                  CREATE UNLOGGED TABLE IF NOT EXISTS "{0}".queue_metric_capture (
                                                      queue_metric_id bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                                                      captured timestamptz NOT NULL,
                                                      queue_id bigint NOT NULL,
                                                      consume_count int NOT NULL,
                                                      error_count int NOT NULL,
                                                      dead_letter_count int NOT NULL
                                                  );

                                                  CREATE TABLE IF NOT EXISTS "{0}".queue_metric (
                                                      queue_metric_id bigint NOT NULL GENERATED BY DEFAULT AS IDENTITY PRIMARY KEY,
                                                      start_time timestamptz NOT NULL,
                                                      duration interval NOT NULL,
                                                      queue_id bigint NOT NULL,
                                                      consume_count bigint NOT NULL,
                                                      error_count bigint NOT NULL,
                                                      dead_letter_count bigint NOT NULL
                                                  );

                                                  SELECT "{0}".create_constraint_if_not_exists('queue_metric', 'unique_queue_metric',
                                                          'CREATE UNIQUE INDEX IF NOT EXISTS queue_metric_ndx ON "{0}".queue_metric (start_time, duration, queue_id);ALTER TABLE "{0}".queue_metric ADD CONSTRAINT unique_queue_metric UNIQUE USING INDEX queue_metric_ndx;');

                                                  CREATE INDEX IF NOT EXISTS queue_metric_queue_id ON "{0}".queue_metric (queue_id, start_time) INCLUDE (duration);

                                                  CREATE OR REPLACE FUNCTION "{0}".process_metrics(row_limit int DEFAULT 10000)
                                                      RETURNS int
                                                      LANGUAGE PLPGSQL
                                                  AS
                                                  $$
                                                  BEGIN
                                                      WITH metrics AS (
                                                          DELETE FROM "{0}".queue_metric_capture
                                                              WHERE queue_metric_id < COALESCE((SELECT MIN(queue_metric_id) FROM "{0}".queue_metric_capture), 0) + row_limit
                                                                 RETURNING *
                                                          )
                                                          INSERT INTO "{0}".queue_metric (start_time, duration, queue_id, consume_count, error_count, dead_letter_count)
                                                              SELECT date_trunc('minute', m.captured),
                                                                     interval '1 minute',
                                                                     m.queue_id,
                                                                     sum(m.consume_count),
                                                                     sum(m.error_count),
                                                                     sum(m.dead_letter_count)
                                                              FROM metrics m
                                                              GROUP BY date_trunc('minute', m.captured), m.queue_id
                                                              ON CONFLICT ON CONSTRAINT unique_queue_metric DO
                                                                  UPDATE SET consume_count = queue_metric.consume_count + excluded.consume_count,
                                                                             error_count = queue_metric.error_count + excluded.error_count,
                                                                             dead_letter_count = queue_metric.dead_letter_count + excluded.dead_letter_count;

                                                      WITH metrics AS (
                                                          DELETE FROM "{0}".queue_metric
                                                              WHERE duration = interval '1 minute' AND start_time < (now() at time zone 'utc') - interval '8 hours'
                                                                 RETURNING *
                                                          )
                                                      INSERT INTO "{0}".queue_metric (start_time, duration, queue_id, consume_count, error_count, dead_letter_count)
                                                          SELECT date_trunc('hour', m.start_time), interval '1 hour', m.queue_id,
                                                              sum(m.consume_count),
                                                              sum(m.error_count),
                                                              sum(m.dead_letter_count)
                                                              FROM metrics m
                                                              GROUP BY date_trunc('hour', m.start_time), m.queue_id
                                                          ON CONFLICT ON CONSTRAINT unique_queue_metric DO
                                                          UPDATE SET consume_count = queue_metric.consume_count + excluded.consume_count,
                                                                     error_count = queue_metric.error_count + excluded.error_count,
                                                                     dead_letter_count = queue_metric.dead_letter_count + excluded.dead_letter_count;

                                                      WITH metrics AS (
                                                          DELETE FROM "{0}".queue_metric
                                                              WHERE duration = interval '1 hour' AND start_time < (now() at time zone 'utc') - interval '48 hours'
                                                                 RETURNING *
                                                          )
                                                      INSERT INTO "{0}".queue_metric (start_time, duration, queue_id, consume_count, error_count, dead_letter_count)
                                                          SELECT date_trunc('day', m.start_time), interval '1 day', m.queue_id,
                                                              sum(m.consume_count),
                                                              sum(m.error_count),
                                                              sum(m.dead_letter_count)
                                                              FROM metrics m
                                                              GROUP BY date_trunc('day', m.start_time), m.queue_id
                                                          ON CONFLICT ON CONSTRAINT unique_queue_metric DO
                                                          UPDATE SET consume_count = queue_metric.consume_count + excluded.consume_count,
                                                                     error_count = queue_metric.error_count + excluded.error_count,
                                                                     dead_letter_count = queue_metric.dead_letter_count + excluded.dead_letter_count;

                                                      DELETE FROM "{0}".queue_metric
                                                          WHERE start_time < (now() at time zone 'utc') - interval '90 days';

                                                      RETURN 0;
                                                  END;
                                                  $$;

                                                  CREATE OR REPLACE FUNCTION "{0}".purge_topology()
                                                      RETURNS int
                                                      LANGUAGE PLPGSQL
                                                  AS
                                                  $$
                                                  BEGIN
                                                      WITH expired AS (SELECT q.id, q.name, (now() at time zone 'utc') - make_interval(secs => q.auto_delete) as expires_at
                                                                       FROM "{0}".queue q
                                                                       WHERE q.auto_delete IS NOT NULL AND (now() at time zone 'utc') - make_interval(secs => q.auto_delete) > updated),
                                                           metrics AS (SELECT qm.queue_id, MAX(start_time) as start_time
                                                                       FROM "{0}".queue_metric qm
                                                                                INNER JOIN expired q2 on q2.id = qm.queue_id
                                                                       WHERE start_time + duration > q2.expires_at
                                                                       GROUP BY qm.queue_id)
                                                      DELETE FROM "{0}".queue qd
                                                             USING (SELECT qdx.id FROM expired qdx WHERE qdx.id NOT IN (SELECT queue_id FROM metrics)) exp
                                                             WHERE qd.id = exp.id;

                                                      RETURN 0;
                                                  END;
                                                  $$;

                                                  CREATE OR REPLACE VIEW "{0}".queues
                                                  AS
                                                  SELECT x.queue_name,
                                                         bool_or(x.queue_auto_delete)                  as queue_auto_delete,
                                                         SUM(x.message_ready)                          as ready,
                                                         SUM(x.message_scheduled)                      as scheduled,
                                                         SUM(x.message_error)                          as errored,
                                                         SUM(x.message_dead_letter)                    as dead_lettered,
                                                         SUM(x.message_locked)                         as locked,
                                                         COALESCE(SUM(x.consume_count), 0)::bigint     as consume_count,
                                                         COALESCE(SUM(x.error_count), 0)::bigint       as error_count,
                                                         COALESCE(SUM(x.dead_letter_count), 0)::bigint as dead_letter_count,
                                                         COALESCE(MAX(x.duration), 0)::int             as count_duration

                                                  FROM (SELECT q.name                                               as queue_name,
                                                               CASE WHEN q.auto_delete = 1 THEN true else false end as queue_auto_delete,
                                                               qm.consume_count,
                                                               qm.error_count,
                                                               qm.dead_letter_count,
                                                               qm.duration,

                                                               CASE
                                                                   WHEN q.type = 1
                                                                       AND md.message_delivery_id IS NOT NULL
                                                                       AND md.enqueue_time <= (now() at time zone 'utc') THEN 1
                                                                   ELSE 0 END                                       as message_ready,
                                                               CASE
                                                                   WHEN q.type = 1
                                                                       AND md.message_delivery_id IS NOT NULL
                                                                       AND md.lock_id IS NULL
                                                                       AND md.enqueue_time > (now() at time zone 'utc') THEN 1
                                                                   ELSE 0 END                                       as message_scheduled,
                                                               CASE
                                                                   WHEN q.type = 1
                                                                       AND md.message_delivery_id IS NOT NULL
                                                                       AND md.lock_id IS NOT NULL
                                                                       AND md.delivery_count >= 1
                                                                       AND md.enqueue_time > (now() at time zone 'utc') THEN 1
                                                                   ELSE 0 END                                       as message_locked,
                                                               CASE
                                                                   WHEN q.type = 2
                                                                       AND md.message_delivery_id IS NOT NULL THEN 1
                                                                   ELSE 0 END                                       as message_error,
                                                               CASE
                                                                   WHEN q.type = 3
                                                                       AND md.message_delivery_id IS NOT NULL THEN 1
                                                                   ELSE 0 END                                       as message_dead_letter
                                                        FROM "{0}".queue q
                                                                 LEFT JOIN "{0}".message_delivery md ON q.id = md.queue_id
                                                                 LEFT JOIN (SELECT DISTINCT ON (qm.queue_id) qm.queue_id,
                                                                                                             q2.name                         as queue_name,
                                                                                                             qm.consume_count                as consume_count,
                                                                                                             qm.error_count                  as error_count,
                                                                                                             qm.dead_letter_count            as dead_letter_count,
                                                                                                             EXTRACT(EPOCH FROM qm.duration) as duration
                                                                            FROM "{0}".queue_metric qm
                                                                                     INNER JOIN "{0}".queue q2 on qm.queue_id = q2.id
                                                                            WHERE q2.type = 1
                                                                              AND qm.start_time >= (now() at time zone 'utc') - interval '1 minutes'
                                                                            ORDER BY qm.queue_id, qm.start_time DESC) qm ON qm.queue_id = q.id) x
                                                  GROUP BY x.queue_name;

                                                  SET ROLE none;
                                                  """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateTable(
                name: "audit_log",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    entity_type_name =
                        table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    operation_type = table.Column<int>(type: "integer", nullable: false),
                    entity_changes = table.Column<string>(type: "jsonb", nullable: true),
                    entity_key = table.Column<string>(type: "jsonb", nullable: true),
                    meta_data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_audit_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event_log",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    timestamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    event_type_name =
                        table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    event_payload = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    meta_data = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event_log", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "inbox_state",
                schema: "shared",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    consumer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    receive_count = table.Column<int>(type: "integer", nullable: false),
                    expiration_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sequence_number = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbox_state", x => x.id);
                    table.UniqueConstraint("ak_inbox_state_message_id_consumer_id",
                        x => new { x.message_id, x.consumer_id });
                });

            migrationBuilder.CreateTable(
                name: "job_attempt_saga",
                schema: "shared",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<int>(type: "integer", nullable: false),
                    job_id = table.Column<Guid>(type: "uuid", nullable: false),
                    retry_attempt = table.Column<int>(type: "integer", nullable: false),
                    service_address = table.Column<string>(type: "text", nullable: true),
                    instance_address = table.Column<string>(type: "text", nullable: true),
                    started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    faulted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status_check_token_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_attempt_saga", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "job_saga",
                schema: "shared",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<int>(type: "integer", nullable: false),
                    submitted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    service_address = table.Column<string>(type: "text", nullable: true),
                    job_timeout = table.Column<TimeSpan>(type: "interval", nullable: true),
                    job = table.Column<string>(type: "text", nullable: true),
                    job_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_id = table.Column<Guid>(type: "uuid", nullable: false),
                    retry_attempt = table.Column<int>(type: "integer", nullable: false),
                    started = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    faulted = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    job_slot_wait_token = table.Column<Guid>(type: "uuid", nullable: true),
                    job_retry_delay_token = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_saga", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "job_type_saga",
                schema: "shared",
                columns: table => new
                {
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_state = table.Column<int>(type: "integer", nullable: false),
                    active_job_count = table.Column<int>(type: "integer", nullable: false),
                    concurrent_job_limit = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    override_job_limit = table.Column<int>(type: "integer", nullable: true),
                    override_limit_expiration =
                        table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    active_jobs = table.Column<string>(type: "text", nullable: true),
                    instances = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_job_type_saga", x => x.correlation_id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_message",
                schema: "shared",
                columns: table => new
                {
                    sequence_number = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy",
                            NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    enqueue_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sent_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    headers = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    inbox_message_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inbox_consumer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    outbox_id = table.Column<Guid>(type: "uuid", nullable: true),
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_type =
                        table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    message_type = table.Column<string>(type: "text", nullable: false),
                    body = table.Column<string>(type: "text", nullable: false),
                    conversation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    correlation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    initiator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    request_id = table.Column<Guid>(type: "uuid", nullable: true),
                    source_address =
                        table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    destination_address =
                        table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    response_address =
                        table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    fault_address =
                        table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    expiration_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_message", x => x.sequence_number);
                });

            migrationBuilder.CreateTable(
                name: "outbox_state",
                schema: "shared",
                columns: table => new
                {
                    outbox_id = table.Column<Guid>(type: "uuid", nullable: false),
                    lock_id = table.Column<Guid>(type: "uuid", nullable: false),
                    row_version = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_sequence_number = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_state", x => x.outbox_id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_entity_type_name_timestamp",
                schema: "shared",
                table: "audit_log",
                columns: new[] { "entity_type_name", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_audit_log_timestamp",
                schema: "shared",
                table: "audit_log",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_event_log_event_type_name_timestamp",
                schema: "shared",
                table: "event_log",
                columns: new[] { "event_type_name", "timestamp" });

            migrationBuilder.CreateIndex(
                name: "ix_event_log_timestamp",
                schema: "shared",
                table: "event_log",
                column: "timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_inbox_state_delivered",
                schema: "shared",
                table: "inbox_state",
                column: "delivered");

            migrationBuilder.CreateIndex(
                name: "ix_job_attempt_saga_job_id_retry_attempt",
                schema: "shared",
                table: "job_attempt_saga",
                columns: new[] { "job_id", "retry_attempt" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_enqueue_time",
                schema: "shared",
                table: "outbox_message",
                column: "enqueue_time");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_expiration_time",
                schema: "shared",
                table: "outbox_message",
                column: "expiration_time");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_inbox_message_id_inbox_consumer_id_sequence_",
                schema: "shared",
                table: "outbox_message",
                columns: new[] { "inbox_message_id", "inbox_consumer_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_message_outbox_id_sequence_number",
                schema: "shared",
                table: "outbox_message",
                columns: new[] { "outbox_id", "sequence_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_state_created",
                schema: "shared",
                table: "outbox_state",
                column: "created");

            migrationBuilder.Sql(string.Format(TransportMigration, "shared"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "event_log",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "inbox_state",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "job_attempt_saga",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "job_saga",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "job_type_saga",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "outbox_message",
                schema: "shared");

            migrationBuilder.DropTable(
                name: "outbox_state",
                schema: "shared");
        }
    }
}
