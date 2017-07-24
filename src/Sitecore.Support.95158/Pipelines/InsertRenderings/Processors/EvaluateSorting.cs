﻿using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines.InsertRenderings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Sitecore.Support.Pipelines.InsertRenderings.Processors
{
    public class EvaluateSorting : InsertRenderingsProcessor
    {
        private int CompareByKey(KeyValuePair<string, string> x, KeyValuePair<string, string> y)
        {
            if (x.Key != y.Value)
            {
                return 0;
            }
            return 1;
        }

        protected virtual void Evaluate(InsertRenderingsArgs args, Item item)
        {
            new List<RenderingReference>(args.Renderings);
            if (item != null)
            {
                LayoutField field = item.Fields[FieldIDs.FinalLayoutField];
                if (!field.InnerField.ContainsStandardValue && !string.IsNullOrEmpty(field.InnerField.Value))
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(field.InnerField.Value);
                    string deviceId = Context.Device.ID.ToString();
                    XmlNode xmlNode = null;
                    this.GetDevice(xmlDocument, deviceId, ref xmlNode);
                    if (xmlNode != null)
                    {
                        List<KeyValuePair<string, string>> sorting = this.GetSorting(xmlNode);
                        sorting.Sort(new Comparison<KeyValuePair<string, string>>(this.CompareByKey));
                        Dictionary<string, string> sortingDictionary = sorting.ToDictionary((KeyValuePair<string, string> keyItem) => keyItem.Key, (KeyValuePair<string, string> valueItem) => valueItem.Value);
                        using (Dictionary<string, string>.KeyCollection.Enumerator enumerator = sortingDictionary.Keys.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                string key = enumerator.Current;
                                RenderingReference reference = args.Renderings.Find(x => x.UniqueId == key);
                                RenderingReference reference2 = args.Renderings.Find(x => x.UniqueId == sortingDictionary[key]);
                                if ((reference != null) && (reference2 != null))
                                {
                                    args.Renderings.Remove(reference);
                                    int index = args.Renderings.IndexOf(reference2);
                                    args.Renderings.Insert(index, reference);
                                }
                            }
                        }
                    }
                }
            }
        }

        protected virtual void GetDevice(XmlNode parentNode, string deviceId, ref XmlNode device)
        {
            if (parentNode.Name == "d")
            {
                device = parentNode;
                return;
            }
            foreach (XmlNode parentNode2 in parentNode.ChildNodes)
            {
                this.GetDevice(parentNode2, deviceId, ref device);
            }
        }

        protected virtual List<KeyValuePair<string, string>> GetSorting(XmlNode device)
        {
            List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
            foreach (XmlNode xmlNode in device.ChildNodes)
            {
                if (xmlNode.Attributes != null)
                {
                    string text = (xmlNode.Attributes["p:before"] != null) ? xmlNode.Attributes["p:before"].Value : null;
                    string text2 = (xmlNode.Attributes["uid"] != null) ? xmlNode.Attributes["uid"].Value : null;
                    if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(text2))
                    {
                        int num = text.IndexOf('{');
                        int num2 = text.IndexOf('}');
                        if (num > 0 && num2 > num)
                        {
                            text = text.Substring(num, num2 - num + 1);
                            list.Add(new KeyValuePair<string, string>(text2, text));
                        }
                    }
                }
            }
            return list;
        }
        public override void Process(InsertRenderingsArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (args.HasRenderings && (Context.Database != Client.CoreDatabase))
            {
                Item contextItem = args.ContextItem;
                if (contextItem != null)
                {
                    this.Evaluate(args, contextItem);
                }
            }
        }
    }
}