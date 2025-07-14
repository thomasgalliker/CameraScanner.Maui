namespace CameraScanner.Maui.Tests.TestData
{
    public static class AddressBookParsedResults
    {
        public static AddressBookParsedResult GetVCardTest1()
        {
            return new AddressBookParsedResult(
                names: new string[] { "FirstName MiddleName LastName" },
                phoneNumbers: new string[] { "HomePhone", "WorkPhone" },
                phoneTypes: new string[] { "HOME,VOICE", "WORK,VOICE" },
                emails: new string[] { "test@mail.com", "work@mail.com" },
                emailTypes: new string[] { "HOME,INTERNET", "WORK,INTERNET" },
                addresses: new string[] { "Street\nCity\nState\nPostalCode\nCountry", "Street\nCity\nState\nPostalCode\nCountry" },
                addressTypes: new string[] { "HOME", "WORK" },
                nicknames: new string[] { "NickName" },
                pronunciation: null,
                instantMessenger: null,
                note: null,
                org: "Organization",
                birthday: null,
                title: "Title",
                urls: new string[] { "http://localhost", "https://work.com" },
                geo: null)
            {
            };
        }
    }
}