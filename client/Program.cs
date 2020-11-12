using Blog;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client
{
    class Program
    {
        const string target = "127.0.0.1:50051";
        public static async Task Main(string[] args)
        {
            Channel channel = new Channel(target, ChannelCredentials.Insecure);
            await channel.ConnectAsync().ContinueWith((task) =>
            {
                if (task.Status == TaskStatus.RanToCompletion)
                    Console.WriteLine("The client connected successfully");
            });

            var client =new BlogService.BlogServiceClient(channel);
            string response = "";

            //response = createBlog(client);
            // response = readBlog(client);
            // response = updateBlog(client,new Blog.Blog());
            // response = deleteBlog(client);
            await ListBlogAsync(client);

            Console.WriteLine(response);
            channel.ShutdownAsync().Wait();
            Console.ReadKey();
        }
        public static string deleteBlog(BlogService.BlogServiceClient client)
        {
            var response = client.DeleteBlog(new DeleteBlogRequest()
            {
                BlogId = "5f9f0f21a772b24eadb150a8"
            });

            return "new Blog" + response.BlogId  + "was Deleted !";
        }
        public static string updateBlog(BlogService.BlogServiceClient client, Blog.Blog blog)
        {
            try
            {

                blog.Id = "5f9f0f21a772b24eadb150a8";
                blog.AuthorId = "Ali";
                blog.Content = "Agah";
                blog.Title = "Welcome To Iran";
                   
                var response = client.UpdateBlog(new UpdateBlogRequest()
                {
                    Blog = blog
                });

                return response.Blog.ToString();
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("client error:" + e.Message);
                throw;
            }
        }
        public static string readBlog(BlogService.BlogServiceClient client)
        {
            try
            {
                var response = client.ReadBlog(new ReadBlogRequest()
                {
                    BlogId = "5f9f0f21a772b24eadb150a8"
                });

                return "Get a Blog : " + response;
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("client error:" + e.Message);
                throw;
            }
        }
        public static async Task ListBlogAsync(BlogService.BlogServiceClient client)
        {
            try
            {
                var response = client.ListBlog(new ListBlogRequest());
                while (await response.ResponseStream.MoveNext())
                {
                    Console.WriteLine(response.ResponseStream.Current.Blog.ToString());
                }
            }
            catch (System.IO.IOException e)
            {
                Console.WriteLine("client error:" + e.Message);
                throw;
            }
        }
        public static string createBlog(BlogService.BlogServiceClient client)
        {
            var response = client.CreateBlog(new CreateBlogRequest()
            {
                Blog = new Blog.Blog()
                {
                    AuthorId = "Ali",
                    Title = "Agah!",
                    Content = "Welcom To Iran"
                }
            });

            return "new Blog" + response.Blog.Id + "was Created !";
        }

    }
}
