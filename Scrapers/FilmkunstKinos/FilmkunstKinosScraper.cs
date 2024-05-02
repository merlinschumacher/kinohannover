﻿using HtmlAgilityPack;
using kinohannover.Data;
using kinohannover.Models;
using Microsoft.Extensions.Logging;

namespace kinohannover.Scrapers.FilmkunstKinos
{
    public abstract class FilmkunstKinosScraper : ScraperBase, IScraper
    {
        private const string contentBoxSelector = "//div[contains(concat(' ', normalize-space(@class), ' '), ' contentbox ')]";
        private const string movieSelector = ".//table";
        private const string titleSelector = ".//h3";
        private const string filmTagSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtag ')]";
        private const string dateSelector = ".//span[contains(concat(' ', normalize-space(@class), ' '), ' filmtagdatum ')]/text()[preceding-sibling::br]";
        private const string timeSelector = ".//a";
        private readonly string name;
        private readonly string website;
        private const string dateFormat = "dd.MM.";
        private readonly HttpClient _httpClient = new();
        private readonly KinohannoverContext context;
        private readonly Cinema cinema;

        public FilmkunstKinosScraper(KinohannoverContext context, string cinemaName, string cinemaUrl, ILogger<FilmkunstKinosScraper> logger) : base(context, logger)
        {
            name = cinemaName;
            website = cinemaUrl;
            this.context = context;
            cinema = CreateCinema(name, website);
        }

        public async Task ScrapeAsync()
        {
            var scrapedHtml = _httpClient.GetAsync(website);
            var html = await scrapedHtml.Result.Content.ReadAsStringAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var contentBox = doc.DocumentNode.SelectSingleNode(contentBoxSelector);
            var movieNodes = contentBox.SelectNodes(movieSelector);
            foreach (var movieNode in movieNodes)
            {
                var title = movieNode.SelectSingleNode(titleSelector).InnerText;
                var movie = CreateMovie(title, cinema);
                movie.Cinemas.Add(cinema);

                var filmTagNodes = movieNode.SelectNodes(filmTagSelector);

                foreach (var filmTagNode in filmTagNodes)
                {
                    var date = filmTagNode.SelectSingleNode(dateSelector);
                    var dateTime = DateOnly.ParseExact(date.InnerText, dateFormat);

                    var timeNodes = filmTagNode.SelectNodes(timeSelector);
                    foreach (var timeNode in timeNodes)
                    {
                        if (!TimeOnly.TryParse(timeNode.InnerText, culture, out var timeOnly)) continue;
                        var showDateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, timeOnly.Hour, timeOnly.Minute, 0);
                        CreateShowTime(movie, showDateTime, cinema);
                    }
                }
            }
            context.SaveChanges();
        }
    }
}
