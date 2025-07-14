using System.Text;
using CameraScanner.Maui.Extensions;

namespace CameraScanner.Maui
{
    public sealed class AddressBookParsedResult : ParsedResult
    {
        public AddressBookParsedResult(
            string[] names,
            string[] phoneNumbers,
            string[] phoneTypes,
            string[] emails,
            string[] emailTypes,
            string[] addresses,
            string[] addressTypes)
            : this(names, null, null, phoneNumbers, phoneTypes, emails, emailTypes,
                null, null, addresses, addressTypes, null, null, null, null, null)
        {
        }

        public AddressBookParsedResult(
            string[] names,
            string[] nicknames,
            string pronunciation,
            string[] phoneNumbers,
            string[] phoneTypes,
            string[] emails,
            string[] emailTypes,
            string instantMessenger,
            string note,
            string[] addresses,
            string[] addressTypes,
            string org,
            string birthday,
            string title,
            string[] urls,
            string[] geo)
        {
            this.Names = names;
            this.Nicknames = nicknames;
            this.Pronunciation = pronunciation;
            this.PhoneNumbers = phoneNumbers;
            this.PhoneTypes = phoneTypes;
            this.Emails = emails;
            this.EmailTypes = emailTypes;
            this.InstantMessenger = instantMessenger;
            this.Note = note;
            this.Addresses = addresses;
            this.AddressTypes = addressTypes;
            this.Org = org;
            this.Birthday = birthday;
            this.Title = title;
            this.URLs = urls;
            this.Geo = geo;
        }

        public string[] Names { get; }
        public string[] Nicknames { get; }
        public string Pronunciation { get; }
        public string[] PhoneNumbers { get; }
        public string[] PhoneTypes { get; }
        public string[] Emails { get; }
        public string[] EmailTypes { get; }
        public string InstantMessenger { get; }
        public string Note { get; }
        public string[] Addresses { get; }
        public string[] AddressTypes { get; }
        public string Org { get; }
        public string Birthday { get; }
        public string Title { get; }
        public string[] URLs { get; }
        public string[] Geo { get; }

        public override string GetDisplayResult()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendIfNotNullOrEmpty(this.Names);
            stringBuilder.AppendIfNotNullOrEmpty(this.Nicknames);
            stringBuilder.AppendIfNotNullOrEmpty(this.Pronunciation);
            stringBuilder.AppendIfNotNullOrEmpty(this.Title);
            stringBuilder.AppendIfNotNullOrEmpty(this.Addresses);
            stringBuilder.AppendIfNotNullOrEmpty(this.Org);
            stringBuilder.AppendIfNotNullOrEmpty(this.PhoneNumbers);
            stringBuilder.AppendIfNotNullOrEmpty(this.Emails);
            stringBuilder.AppendIfNotNullOrEmpty(this.InstantMessenger);
            stringBuilder.AppendIfNotNullOrEmpty(this.URLs);
            stringBuilder.AppendIfNotNullOrEmpty(this.Birthday);
            stringBuilder.AppendIfNotNullOrEmpty(this.Geo);
            stringBuilder.AppendIfNotNullOrEmpty(this.Note);

            return stringBuilder.ToString();
        }
    }
}