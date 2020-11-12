using Blog;
using Grpc.Core;
using Grpc.Reflection;
using Grpc.Reflection.V1Alpha;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Program
    {
        const int port = 50051;
        static void Main(string[] args)
        {
           
            Server server = null;
            try
            {
                var reflectionServiceImpl = new ReflectionServiceImpl(BlogService.Descriptor, ServerReflection.Descriptor);

                server = new Server()
                {
                    Services = {
                        BlogService.BindService(new BlogServiceImpl()),
                        ServerReflection.BindService(reflectionServiceImpl)
                    },
                    Ports = { new ServerPort("localhost", port, ServerCredentials.Insecure) }
                };
                server.Start();
                Console.WriteLine("The server is listenin on port " + port);
                Console.ReadKey();
            }
            catch (IOException e)
            {
                Console.WriteLine("The server faild" + e.Message);
                throw;
            }
            finally
            {
                if (server != null)
                {
                    server.ShutdownAsync().Wait();
                }
            }
        }
    }
}
