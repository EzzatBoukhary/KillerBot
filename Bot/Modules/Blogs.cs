using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bot.Configuration;
using Bot.Extensions;
using Bot.Features.Blogs;
using Bot.Handlers;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Bot.Modules
{
    [Group("Blog"), Summary("Enables you to create a blog that people can subscribe to so they don't miss out your posts.")]
    public class Blogs : ModuleBase<MiunieCommandContext>
    {
        private readonly JsonDataStorage jsonDataStorage;
        private readonly BlogHandler blogHandler;
        private static readonly string blogFile = "blogs.json";
        public Blogs(JsonDataStorage jsonDataStorage, BlogHandler blogHandler)
        {
            this.jsonDataStorage = jsonDataStorage;
            this.blogHandler = blogHandler;
        }

        [Command("Create"), Remarks("Create a new named blog")]
        public async Task Create(string name)
        {

            var blogs = jsonDataStorage.RestoreObject<List<BlogItem>>(blogFile) ?? new List<BlogItem>();

            if (blogs.FirstOrDefault(k=>k.Name == name) == null)
            {
                var newBlog = new BlogItem
                {
                    BlogId = Guid.NewGuid(),
                    Author = Context.User.Id,
                    Name = name,
                    Subscribers = new List<ulong>()
                };

                blogs.Add(newBlog);

                jsonDataStorage.StoreObject(blogs, blogFile, Formatting.Indented);
                
                var embed = EmbedHandler.CreateEmbed("Blog", $"Your blog {name} was created.", EmbedHandler.EmbedMessageType.Success);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
            else
            {
                var embed = EmbedHandler.CreateEmbed("Blog :x:", $"There is already a Blog with the name {name}", EmbedHandler.EmbedMessageType.Error);
                await Context.Channel.SendMessageAsync("", false, embed);
            }
        }

        [Command("Post"), Remarks("Publish a new post to one of your named blogs")]
        public async Task Post(string name, [Remainder]string post)
        {

            var blogs = jsonDataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.Name == name && k.Author == Context.User.Id);

            if (blog != null)
            {
                var subs = string.Empty;
                foreach (var subId in blog.Subscribers)
                {
                    var sub = Context.Guild.GetUser(subId);
                    
                    subs += $"{sub.Username}#{sub.Discriminator},";
                }
               
                if (string.IsNullOrEmpty(subs))
                {
                    subs = "No subscribers";
                }

                var embed = EmbedHandler.CreateBlogEmbed(blog.Name, post, subs, EmbedHandler.EmbedMessageType.Info, true);
                var msg = Context.Channel.SendMessageAsync("", false, embed);
                
                if (Global.MessagesIdToTrack == null)
                {
                    Global.MessagesIdToTrack = new Dictionary<ulong, string>();
                }

                Global.MessagesIdToTrack.Add(msg.Result.Id, blog.Name);

                await msg.Result.AddReactionAsync(new Emoji("➕"));
                foreach (var subscriber in blog.Subscribers)
                    try
                {
                        var dm = await Context.Client.GetUser(subscriber).GetOrCreateDMChannelAsync();
                        var dmmsg = EmbedHandler.CreateEmbed($"New post from a subscribed blog \"{blog.Name}\": ", post, EmbedHandler.EmbedMessageType.Info, true);
                    await dm.SendMessageAsync("", false, dmmsg);
                    }

                catch (HttpException ignored) when (ignored.DiscordCode == 50007) { }


            }
        }

        [Command("Subscribe"), Remarks("Subscribe to a named blog to receive a message when a new post gets published")]
        public async Task Subscribe(string name)
        {

            var embed = blogHandler.SubscribeToBlog(Context.User.Id, name);

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("Unsubscribe"), Remarks("Remove a subscription from a named block")]
        public async Task UnSubscribe(string name)
        {

            var embed = blogHandler.UnSubscribeToBlog(Context.User.Id, name);

            await Context.Channel.SendMessageAsync("", false, embed);
        }
        
      /*  [Command("delete")]
        public async Task DeleteBlog(Guid ID)
        {
            if (ID == null)
                throw new ArgumentException("ree");

            await Context.Message.DeleteAsync();

            var blogs = jsonDataStorage.RestoreObject<List<BlogItem>>(blogFile);

            var blog = blogs.FirstOrDefault(k => k.BlogId == ID && k.Author == Context.User.Id);
            var embed = EmbedHandler.CreateBlogEmbed(blog.Name, "You just deleted this blog.", "", EmbedHandler.EmbedMessageType.Info, true);

            if (blog != null)
            {
                blogs.Remove(blog);
            } 
                await Context.Channel.SendMessageAsync("", false, embed);
            

        } */
    }
}
