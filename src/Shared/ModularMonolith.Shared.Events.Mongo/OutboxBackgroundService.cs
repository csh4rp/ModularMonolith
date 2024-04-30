using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModularMonolith.Shared.Application.Abstract;
using ModularMonolith.Shared.Contracts;
using ModularMonolith.Shared.Events.Mongo.Entities;
using ModularMonolith.Shared.Events.Mongo.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace ModularMonolith.Shared.Events.Mongo;
