DO
$$

DECLARE
new_user_name CONSTANT VARCHAR(128) := 'mail@mail.com';

-- Pa$$word123!@#
DECLARE
new_user_password_hash CONSTANT VARCHAR(255) := 'AQAAAAIAAYagAAAAEExLl8UzbmjdreSVh9HhneMNcOVzEnKMCKFNyym/N1y8evJUbsENt3dBLFyxUqBO4A==';

BEGIN

IF
NOT EXISTS (SELECT 1 FROM "identity"."user" WHERE user_name = new_user_name) THEN

    INSERT INTO "identity"."user"
    (id, user_name, normalized_user_name, email, normalized_email, password_hash, access_failed_count, concurrency_stamp,
     security_stamp, lockout_enabled, two_factor_enabled, phone_number_confirmed, email_confirmed)
    VALUES (gen_random_uuid(), new_user_name, upper(new_user_name), new_user_name, upper(new_user_name),
            new_user_password_hash, 0, gen_random_uuid()::text,
           gen_random_uuid()::text, false, false, false, false);
END IF;

END $$
