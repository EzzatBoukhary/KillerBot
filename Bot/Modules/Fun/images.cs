using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;


using Discord;
using Discord.Commands;
using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using System.Net.Http;

namespace Bot.Modules.Fun
{
    public class Images : ModuleBase<SocketCommandContext>
    {

        [Command("resize"), Alias("rz"), Summary("Resize a image")]
        public async Task ResizeImage([Summary("URL of the image to resize")] string URL = null,
            [Summary("The Y/Width to rezize to")] int YVal = 0,
            [Summary("The X/Height to resize to")] int XVal = 0)

        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            { 
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
            return;
        }
            await ReplyAsync("Processing... This might take some time.");
            Size imgSize = new Size(YVal, XVal);
            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));
            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {

                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                        .Resize(new ImageProcessor.Imaging.ResizeLayer(imgSize, ImageProcessor.Imaging.ResizeMode.Stretch))
                        .Format(new JpegFormat { Quality = 100 })
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "resized.jpg", text: $"{outStream.Length / 1024} KB");
                    return;
                }
            }
            await ReplyAsync("Something bad happened... Bad URL perhaps?");
            return;


        }

        [Command("jpeg"), Alias("jpegify"), Summary("JPEG an image")]
        public async Task Jpegify([Summary("URL to jpegify")] string URL = null,
            [Summary("JPEG quality")] int quality = 100)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");
            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));
            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                            .Format(new JpegFormat { Quality = quality })
                            .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "jpegified.jpg", text: $"Quality:{quality}  {outStream.Length / 1024}KB");
                    return;
                }
            }
            await ReplyAsync("Something bad happened... Bad URL perhaps?");
            return;
        }

        [Command("fliph"), Summary("Flip an image horizontally")]
        public async Task FlipImgH([Summary("URL to flip")] string URL = null)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");

            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));

            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                        .Flip(false, false)
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "fliph.jpg");
                    return;
                }
            }
            await ReplyAsync("Something went wrong with image downloading...");
            return;
        }

        [Command("flipv"), Alias("flipimg"), Summary("Flip an image vertically")]
        public async Task FlipImgV([Summary("URL to flip")] string URL = null)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");

            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));

            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                        .Flip(true, false)
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "flipv.jpg");
                    return;
                }

            }
            await ReplyAsync("Something went wrong with image downloading...");
            return;
        }

        [Command("pixelate"), Alias("pixel"), Summary("Pixelate an image to x amount of pixels")]
        public async Task Pixelate([Summary("URL to flip")] string URL = null,
            [Summary("Amount to pixelate by (number)")] int pixelSize = 5)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");

            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));

            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                        .Pixelate(pixelSize: pixelSize)
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "pixelated.jpg", $"Pixelated by {pixelSize}px");
                    return;
                }
            }
            await ReplyAsync("Something went wrong with image downloading...");
            return;
        }

        [Command("quality"), Alias("imgq"), Summary("Change the quality of an image")]
        public async Task Quality([Summary("URL to change quality of")] string URL = null,
            [Summary("The percentage of quality")] int qualPerc = 100)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            if (qualPerc > 100 || qualPerc < 0)
            {
                await ReplyAsync("Quality must be between 0 and 100");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");

            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));

            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                        .Quality(qualPerc)
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, $"quality{qualPerc}.jpg", $"Changed quality to {qualPerc}%");
                    return;
                }
            }
            await ReplyAsync("Something went wrong with image downloading...");
            return;
        }

        [Command("deepfry"), Alias("df"), Summary("Attempt to deepfry something....")]
        public async Task AttemptDeepFry([Summary("URL to deepfry")] string URL = null,
            [Summary("Iterations in for")] int iters = 10)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");

            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));

            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    ImageFactory imgFac = Bot.Helpers.HelperFuncs.GetImageFactory(imgStream);

                    for (int i = 0; i <= iters; i++)
                    {
                        imgFac.Saturation(Global.Rng.Next(0, 50))
                            .Quality(Global.Rng.Next(0, 50))
                            .Pixelate(Global.Rng.Next(0, 2));
                    }
                    imgFac.Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "deepfryme.jpg", $"{iters} iterations");
                    return;
                }

            }
            await ReplyAsync("Something went wrong with image downloading...");
            return;
        }

        [Command("detectedges"), Alias("detecte"), Summary("Detect edges in an image using various filters")]
        public async Task ApplyFilter([Summary("URL to filter")] string URL = null,
            [Summary("OPTIONAL: Filter to apply in integer 0-8")] int filter = 0,
            [Summary("OPTIONAL: Apply greyscale or not (true or false)")] bool greyscale = false)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");

            if (filter > 8 || filter < 0)
            {
                await ReplyAsync("You need to enter a number between 0 and 8 for this command...");
                return;
            }

            IEdgeFilter imgFilter = null;
            switch (filter)
            {
                case 0:
                    imgFilter = new KayyaliEdgeFilter();
                    break;

                case 1:
                    imgFilter = new KirschEdgeFilter();
                    break;

                case 2:
                    imgFilter = new Laplacian3X3EdgeFilter();
                    break;

                case 3:
                    imgFilter = new Laplacian5X5EdgeFilter();
                    break;

                case 4:
                    imgFilter = new LaplacianOfGaussianEdgeFilter();
                    break;

                case 5:
                    imgFilter = new PrewittEdgeFilter();
                    break;

                case 6:
                    imgFilter = new RobertsCrossEdgeFilter();
                    break;

                case 7:
                    imgFilter = new ScharrEdgeFilter();
                    break;

                case 8:
                    imgFilter = new SobelEdgeFilter();
                    break;

                default:
                    imgFilter = new KayyaliEdgeFilter();
                    break;
            }

            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));

            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                        .DetectEdges(imgFilter, greyscale)
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "edgedetect.jpg", $"Using filter {imgFilter.ToString()}");
                    return;
                }
            }
            await ReplyAsync("Something went wrong downloading the image... Bad URL perhaps?");
            return;

        }

        [Command("rotate"), Alias("rot"), Summary("Rotate an image clockwise (positive) or counter (negative) degrees")]
        public async Task RotateImg([Summary("URL to rotate")] string URL = null,
            [Summary("Degrees to rotate by (int)")] int degrees = 0)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            await ReplyAsync("Processing... This might take some time.");

            MemoryStream memoryStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));

            if (memoryStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(memoryStream)
                        .Rotate(degrees)
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, $"rotate{degrees.ToString()}.jpg");
                    return;
                }
            }

            await ReplyAsync("Something went wrong... Bad URL perhaps?");
            return;
        }
        [Command("watermark"), Alias("wm"), Summary("Add a watermark to an image")]
        public async Task Watermark([Summary("URL to Watermark")] string URL = null,
            [Summary("Name of the text's color")] string name = "white",
            //[Summary("RGB values separated by space")] int r, int g, int b,
            [Summary("Watermark text"), Remainder] string wmText = null)
        {
            if (URL == null)
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            else if (URL.Contains(".gif"))
            {
                await ReplyAsync($"Sorry, {Context.User.Mention}! Image commands don't work on GIFs.");
                return;
            }
            else if (!URL.Contains("https://"))
            {
                await ReplyAsync("Please insert a URL of an image. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            if (wmText == null)
            {
                await ReplyAsync("Please specify your watermark text. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                    return;
            }
            await ReplyAsync("Processing... This might take some time.");

            MemoryStream imgStream = new MemoryStream(await Bot.Helpers.HelperFuncs.DownloadFileBytesAsync(URL));


            if (imgStream != null)
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    Bot.Helpers.HelperFuncs.GetImageFactory(imgStream)
                        .Watermark(new ImageProcessor.Imaging.TextLayer
                        {
                            Text = wmText,
                            FontColor = System.Drawing.Color.FromName(name),
                            FontSize = 64,
                            DropShadow = false,
                        })
                        .Save(outStream);

                    await Context.Channel.SendFileAsync(outStream, "watermarked.jpg");
                    return;
                }
            }
            await ReplyAsync("Something bad happened. Bad URL perhaps?");
            return;
        }
        public static readonly HttpClient httpClient = new HttpClient();
        [Command("minecraftach"), Alias("mcach", "ach"), Summary("Generate an epic minecraft echievement with customizable yellow and white text")]
        [Remarks("Make sure to put quotes \"\" for the yellow text if you need it to be a sentence. However there's no need to put it for the white text.")]
        public async Task MCAchievmentACH([Summary("The yellow text at the top of the achievement")] string yellowtext = null,
            [Remainder, Summary("The white text at the bottom of the achievment")] string text =  null)
        {
            if (yellowtext == null)
            {
                await ReplyAsync("Please specify what your yellow text is. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            if (text == null)
            {
                await ReplyAsync("Please specify what your white text is. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            var icon = Global.Rng.Next(1, 39);

            using (var response = await httpClient.GetStreamAsync($"https://mcgen.herokuapp.com/a.php?i={icon}&h={yellowtext}&t={text}"))
            {
                await Context.Channel.SendFileAsync(response, "achievement.png");
            }
        }
        [Command("minecraft"), Alias("mc"), Summary("Generate an epic minecraft echievement with customizable white text only.")]
        public async Task MCAchievment([Remainder, Summary("The white text at the bottom of the achievment")] string text = null)
        {
            if (text == null)
            {
                await ReplyAsync("Please specify what your white text is. \n \n**TIP: Command usage can be found in `k!help [command name]`**");
                return;
            }
            var icon = Global.Rng.Next(1, 39);

            using (var response = await httpClient.GetStreamAsync($"https://mcgen.herokuapp.com/a.php?i={icon}&h=Achievement Get!&t={text}"))
            {
                await Context.Channel.SendFileAsync(response, "achievement.png");
            }
        }
    }
}