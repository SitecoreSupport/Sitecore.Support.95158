namespace Sitecore.Support.ExperienceEditor.Speak.Server.Contexts
{
    using Newtonsoft.Json;
    using Sitecore;
    using Sitecore.Collections;
    using Sitecore.Data;
    using Sitecore.Data.Fields;
    using Sitecore.Data.Items;
    using Sitecore.Data.Validators;
    using Sitecore.Diagnostics;
    using Sitecore.ExperienceEditor.Speak.Server.Contexts;
    using Sitecore.ExperienceEditor.Utils;
    using Sitecore.Pipelines.Save;
    using Sitecore.Shell.Applications.WebEdit.Commands;
    using Sitecore.Web;
    using Sitecore.Xml;
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class PageContextNoLayoutDelta : ItemContext
    {
        public static void AddLayoutField(string layout, Packet packet, Item item, string fieldId = null)
        {
            Assert.ArgumentNotNull(packet, "packet");
            Assert.ArgumentNotNull(item, "item");
            if (fieldId == null)
            {
                fieldId = FieldIDs.FinalLayoutField.ToString();
            }
            if (!string.IsNullOrEmpty(layout))
            {
                layout = WebEditUtil.ConvertJSONLayoutToXML(layout);
                Assert.IsNotNull(layout, layout);
                packet.StartElement("field");
                packet.SetAttribute("itemid", item.ID.ToString());
                packet.SetAttribute("language", item.Language.ToString());
                packet.SetAttribute("version", item.Version.ToString());
                packet.SetAttribute("fieldid", fieldId);
                packet.AddElement("value", layout, new string[0]);
                packet.EndElement();
            }
        }

        public static SaveArgs GenerateSaveArgs(Item contextItem, IEnumerable<PageEditorField> fields, string postAction, string layoutValue, string validatorsKey, string fieldId = null)
        {
            SafeDictionary<FieldDescriptor, string> dictionary;
            Packet packet = WebUtility.CreatePacket(contextItem.Database, fields, out dictionary);
            if (WebEditUtil.CanDesignItem(contextItem))
            {
                AddLayoutField(layoutValue, packet, contextItem, fieldId);
            }
            if (!string.IsNullOrEmpty(validatorsKey))
            {
                ValidatorsMode mode;
                ValidatorCollection validators = PipelineUtil.GetValidators(contextItem, dictionary, out mode);
                validators.Key = validatorsKey;
                ValidatorManager.SetValidators(mode, validatorsKey, validators);
            }
            return new SaveArgs(packet.XmlDocument) { 
                SaveAnimation = false,
                PostAction = postAction,
                PolicyBasedLocking = true
            };
        }

        public SafeDictionary<FieldDescriptor, string> GetControlsToValidate()
        {
            Item item = base.Item;
            Assert.IsNotNull(item, "The item is null.");
            IEnumerable<PageEditorField> fields = WebUtility.GetFields(item.Database, this.FieldValues);
            SafeDictionary<FieldDescriptor, string> dictionary = new SafeDictionary<FieldDescriptor, string>();
            foreach (PageEditorField field in fields)
            {
                Item item2 = (item.ID == field.ItemID) ? item : item.Database.GetItem(field.ItemID);
                Field field2 = item.Fields[field.FieldID];
                string str = WebUtility.HandleFieldValue(field.Value, field2.TypeKey);
                FieldDescriptor descriptor = new FieldDescriptor(item2.Uri, field2.ID, str, false);
                string str2 = field.ControlId ?? string.Empty;
                dictionary[descriptor] = str2;
                if (!string.IsNullOrEmpty(str2))
                {
                    RuntimeValidationValues.Current[str2] = str;
                }
            }
            return dictionary;
        }

        public SaveArgs GetSaveArgs()
        {
            IEnumerable<PageEditorField> fields = WebUtility.GetFields(base.Item.Database, this.FieldValues);
            string postAction = string.Empty;
            string layoutSource = this.LayoutSource;
            SaveArgs args = GenerateSaveArgs(base.Item, fields, postAction, layoutSource, string.Empty, WebUtility.GetCurrentLayoutFieldId().ToString());
            args.HasSheerUI = false;
            new ParseXml().Process(args);
            return args;
        }

        [JsonProperty("scFieldValues")]
        public Dictionary<string, string> FieldValues { get; set; }

        [JsonProperty("scLayout")]
        public string LayoutSource { get; set; }

        [JsonProperty("scValidatorsKey")]
        public string ValidatorsKey { get; set; }
    }
}

