
using Blog;
using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Blog.BlogService;
namespace server
{
    class BlogServiceImpl : BlogServiceBase
    {
        private static MongoClient mongoclient = new MongoClient("mongodb://localhost:27017");
        private static IMongoDatabase MongoDatabase = mongoclient.GetDatabase("mydb");
        private static IMongoCollection<BsonDocument> mongoCollection = MongoDatabase.GetCollection<BsonDocument>("blog");

        public override Task<CreateBlogResponse> CreateBlog(CreateBlogRequest request, ServerCallContext context)
        {
            var blog = request.Blog;
            BsonDocument doc = new BsonDocument("auther_id", blog.AuthorId)
                .Add("title", blog.Title)
                .Add("Content", blog.Content);

            mongoCollection.InsertOne(doc);
            
            String id = doc.GetValue("_id").ToString();

            blog.Id = id;

            return Task.FromResult(new CreateBlogResponse()
            {
                Blog = blog
            });
        }

        public override async Task<ReadBlogResponse> ReadBlog(ReadBlogRequest request, ServerCallContext context)
        {
            var blogId = request.BlogId;
            
            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = mongoCollection.Find(filter).FirstOrDefault();
            
            
            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id" + blogId + "wasn't find"));
            }

            Blog.Blog blog = new Blog.Blog();
            blog.AuthorId = result.GetValue("auther_id").AsString;
            blog.Title = result.GetValue("title").AsString;
            blog.Content = result.GetValue("Content").AsString;
            return new ReadBlogResponse() { Blog = blog };

        }
        public override async Task<UpdateBlogResponse> UpdateBlog(UpdateBlogRequest request, ServerCallContext context)
        {
            var blogId = request.Blog.Id;

            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = mongoCollection.Find(filter).FirstOrDefault();


            if (result == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id" + blogId + "wasn't find"));
            }

            var doc = new BsonDocument("auther_id", request.Blog.AuthorId)
                .Add("title", request.Blog.Title)
                .Add("Content", request.Blog.Content);

            mongoCollection.ReplaceOne(filter, doc);

            var blog = new Blog.Blog()
            {
                AuthorId = doc.GetValue("auther_id").AsString,
                Title = doc.GetValue("title").AsString,
                Content = doc.GetValue("Content").AsString
            };
            blog.Id = blogId;
            return new UpdateBlogResponse() { Blog = blog };
        }

        public override async Task<DeleteBlogResponse> DeleteBlog(DeleteBlogRequest request, ServerCallContext context)
        {
            var blogId = request.BlogId;

            var filter = new FilterDefinitionBuilder<BsonDocument>().Eq("_id", new ObjectId(blogId));
            var result = mongoCollection.DeleteOne(filter);


            if (result.DeletedCount == 0)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "The blog id" + blogId + "wasn't find"));
            }

            return new DeleteBlogResponse() { BlogId = blogId };
        }

        public override async Task ListBlog(ListBlogRequest request, IServerStreamWriter<ListBlogResponse> responseStream, ServerCallContext context)
        {
            var filter = new FilterDefinitionBuilder<BsonDocument>().Empty;
            var result = mongoCollection.Find(filter);

            foreach(var item in result.ToList())
            {
                await responseStream.WriteAsync(new ListBlogResponse()
                {
                    Blog = new Blog.Blog()
                    {
                        Id = item.GetValue("_id").ToString(),
                        AuthorId = item.GetValue("auther_id").AsString,
                        Content = item.GetValue("Content").AsString,
                        Title = item.GetValue("title").AsString
                    }
                });
            }
        }
    }
}
