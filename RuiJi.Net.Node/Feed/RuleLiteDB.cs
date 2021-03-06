﻿using LiteDB;
using RuiJi.Net.Core.Extensions.System;
using RuiJi.Net.Core.Utils;
using RuiJi.Net.Core.Utils.Page;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RuiJi.Net.Node.Feed
{
    public class RuleLiteDB
    {
        public static List<RuleModel> GetRuleModels(Paging page)
        {
            using (var db = new LiteDatabase(@"Rules.db"))
            {
                var col = db.GetCollection<RuleModel>("rules");

                page.Count = col.Count();

                return col.Find(Query.All(), page.Start, page.PageSize).ToList();
            }
        }

        public static void AddOrUpdate(RuleModel rule)
        {
            using (var db = new LiteDatabase(@"Rules.db"))
            {
                var col = db.GetCollection<RuleModel>("rules");
                if (Uri.IsWellFormedUriString(rule.Url, UriKind.Absolute))
                    rule.Domain = new Uri(rule.Url).GetDomain();

                if (rule.Id == 0)
                {
                    rule.Url = rule.Url.Trim().ToLower();
                    rule.Expression = rule.Expression.Trim().ToLower();

                    col.Insert(rule);
                }
                else
                {
                    col.Update(rule);
                }
            }
        }

        public static void Remove(int id)
        {
            using (var db = new LiteDatabase(@"Rules.db"))
            {
                var col = db.GetCollection<RuleModel>("rules");
                col.Delete(id);
            }
        }

        public static void CreateIndex()
        {
            using (var db = new LiteDatabase(@"Rules.db"))
            {
                var col = db.GetCollection<RuleModel>("feeds");
                col.EnsureIndex(m => m.Domain);
            }
        }

        public static List<RuleModel> Match(string url)
        {
            url = url.Trim().ToLower();
            var domain = new Uri(url).GetDomain();

            using (var db = new LiteDatabase(@"Rules.db"))
            {
                var col = db.GetCollection<RuleModel>("rules");
                var rules = col.Find(Query.Where("Domain", m => m.AsString == domain)).ToList();
                var masks = rules.Select(m => m.Expression).ToArray();

                var mask = Wildcard.MaxMatch(url, masks);
                return rules.Where(m => m.Expression == mask).ToList();
            }
        }

        public static RuleModel GetRule(int id)
        {
            using (var db = new LiteDatabase(@"Rules.db"))
            {
                var col = db.GetCollection<RuleModel>("rules");

                return col.Find(m => m.Id == id).FirstOrDefault();
            }
        }
    }
}