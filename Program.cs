using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections;

namespace many_to_many
{
    public class Post
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Tag> Tags { get; set; }
    }
    public class Tag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Post> Posts { get; set; }
    }

    public class DatabaseContext: DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer("Data Source=127.0.0.1,1433;Initial Catalog=nn_relation;User ID=sa;Password=reallyStrongPwd123;");
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new DatabaseContext())
            {
                Console.WriteLine("EF Model is: ");
                Console.WriteLine(context.Model.ToDebugString(MetadataDebugStringOptions.ShortDefault));
                var paramsTags = new List<string> { "drupal", "tag 1" };
                // Update post
                var post = context.Posts.Include(e => e.Tags).Single(x => x.Id == 2);
                foreach (var tag in post.Tags)
                {
                    // Remove tag not in list
                    if (!paramsTags.Contains(tag.Name)) {
                        post.Tags.Remove(tag);
                    }
                }
                foreach (var tag in paramsTags)
                {
                    if (!post.Tags.Any(x => x.Name == tag))
                    {
                        var newTag = context.Tags.FirstOrDefault(x => x.Name == tag);
                        if (newTag == null)
                        {
                            newTag = new Tag { Name = tag };
                        }
                        context.Tags.Attach(newTag);
                        post.Tags.Add(newTag);
                    }
                }
                context.SaveChanges();
                Console.WriteLine(context.Tags.FirstOrDefault(x => x.Name == "aaa"));

                // Get lists
                // var posts = context.Posts.Include(e => e.Tags).ToList();
                // Console.WriteLine(posts);
                // foreach (var post in posts)
                // {
                //     Console.WriteLine($"Post: \"{post.Name}\"");
                //     foreach (var tag in post.Tags)
                //     {
                //         Console.WriteLine($"Tag: \"{tag.Name}\"");
                //     }
                // }

                // Create new
                // var post = new Post
                // {
                //     Name = "Test",
                //     Tags = new List<Tag>
                //     {
                //         new Tag {
                //             Name = "tag 1"
                //         },
                //         new Tag {
                //             Name = "tag 2"
                //         }
                //     }
                // };
                // context.Posts.Add(post);
                // context.SaveChanges();
            }

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
