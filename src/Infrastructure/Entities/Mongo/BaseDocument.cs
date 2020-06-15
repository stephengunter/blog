using Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Infrastructure.Entities.Mongo
{
	public interface IDocument
	{
		[BsonId]
		[BsonRepresentation(BsonType.String)]
		ObjectId Id { get; set; }

		DateTime LastUpdated { get; set; }
		DateTime CreatedAt { get; }
	}

	public abstract class BaseDocument : IDocument
	{
		public ObjectId Id { get; set; }

		public string Content { get; set; } //json string

		public DateTime LastUpdated { get; set; } = DateTime.Now;

		public DateTime CreatedAt { get; set; } = DateTime.Now;
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class BsonCollectionAttribute : Attribute
	{
		public string CollectionName { get; }

		public BsonCollectionAttribute(string collectionName)
		{
			CollectionName = collectionName;
		}
	}

}