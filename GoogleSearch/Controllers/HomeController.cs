using GoogleSearch.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace GoogleSearch.Controllers
{
    public class HomeController : Controller
    {
        private GoogleSearchEntities db = new GoogleSearchEntities();
        public ActionResult Index()
        {
            return View(new List<SearchResult>());
        }

        public ActionResult Search(string searchText)
        {
            //format the search query
            searchText = searchText.Replace(' ', '+');
            string searchUrl = "http://www.google.com/search?num=100&q=" + searchText;

            var searchSummery = new SearchSummery();
            searchSummery.SearchTerm = searchText.Replace('+', ' '); ;
            searchSummery.SearchTime = DateTime.Now;
            searchSummery.ResultCount = 5;

            //start timer
            var timer = new Stopwatch();
            timer.Start();

            //initialize web client
            WebClient webClient = new WebClient();

            var result = new HtmlWeb().Load(searchUrl);
            var nodes = result.DocumentNode.SelectNodes("//html//body//div[@class='g']");

            timer.Stop();
            searchSummery.Duration = timer.Elapsed;
            ViewBag.Duration = timer.Elapsed.Milliseconds;

            var searchItems = new List<SearchResult>();

            foreach (var node in nodes.Take(5)) //take 1st 5 results
            {
                var url = node.Descendants("a").FirstOrDefault().Attributes["href"].Value;
                var title = node.Descendants("a").FirstOrDefault().Descendants("div").FirstOrDefault() != null ? node.Descendants("a").FirstOrDefault().Descendants("div").FirstOrDefault().InnerText : node.Descendants("a").FirstOrDefault().InnerText;
                var decription = node.Descendants("div").ElementAtOrDefault(6) != null ? node.Descendants("div").ElementAtOrDefault(6).InnerText : node.Descendants("div").ElementAtOrDefault(4).InnerText;
                var searchItem = new SearchResult()
                {
                    Title = title,
                    Url = url,
                    Description = decription,
                };

                searchItems.Add(searchItem);
            }

            //save the result summery to database
            db.SearchSummeries.Add(searchSummery);
            db.SaveChanges();

            //load search result in partial view
            return PartialView("_searchResultPartial", searchItems);
        }

        public ActionResult SearchHistory()
        {
            //get search history from database
            var searchSummery = db.SearchSummeries.OrderByDescending(s => s.SearchTime).ToList();
            return View(searchSummery);
        }
    }
}