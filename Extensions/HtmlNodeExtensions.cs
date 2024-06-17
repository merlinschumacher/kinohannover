﻿using HtmlAgilityPack;

namespace kinohannover.Extensions
{
    public static class HtmlNodeExtensions
    {
        public static string GetHref(this HtmlNode node)
        {
            return node.GetAttributeValue("href", "");
        }
    }
}