using Bogus;
using FakeUsers.Server.Models;

namespace FakeUsers.Server.Utils
{

    public static class FakeDataGenerator
    {

        public const int PAGE_SIZE = 20;

        private const string USA_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        private const string POLAND_ALPHABET = "AĄBCĆDEĘFGHIJKLŁMNŃOÓPRSŚTUWYZŹŻabcdefghijklmnoprstuwyzźżąćęłńóś";
        private const string UKRAINIAN_ALPHABET = "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя";
        private const string DIGITS = "0123456789";

        public static IEnumerable<User> GenerateData(Region region, double errorCount, string seed, int page, int pageSize = PAGE_SIZE)
        {
            var faker = CreateFaker(region, page, pageSize);
            var seedValue = CombineSeedWithPage(seed, page, pageSize);
            var data = faker.UseSeed(seedValue).Generate(pageSize);

            return data.Select((user, index) => IntroduceErrors(user, errorCount, seedValue + index, region));
        }


        private static Faker<User> CreateFaker(Region region, int page = 0, int pageSize = PAGE_SIZE)
        {
            var (locale, phoneFormat, addressFormat) = GetRegionSettings(region);

            return new Faker<User>(locale)
                .RuleFor(u => u.Number, f => f.IndexFaker + 1 + pageSize * page)
                .RuleFor(u => u.Id, f => f.Random.Guid().ToString())
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.Address, f => GenerateAddress(f, addressFormat))
                .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber(phoneFormat));
        }

        private static (string locale, string phoneFormat, Func<Faker, string>[] addressFormats) GetRegionSettings(Region region)
        {
            return region switch
            {
                Region.USA => ("en_US", "+1 ###-###-####",
                [
                    f => $"{f.Address.StreetAddress()}, {f.Address.City()}, {f.Address.StateAbbr()} {f.Address.ZipCode()}",
                    f => $"{f.Address.BuildingNumber()} {f.Address.StreetName()}, Apt. {f.Random.Number(1, 999)}, {f.Address.City()}, {f.Address.StateAbbr()} {f.Address.ZipCode()}",
                    f => $"P.O. Box {f.Random.Number(1000, 9999)}, {f.Address.City()}, {f.Address.StateAbbr()} {f.Address.ZipCode()}",
                    f => $"{f.Address.StreetAddress()}, {f.Address.SecondaryAddress()}, {f.Address.City()}, {f.Address.State()} {f.Address.ZipCode()}"
                ]),
                Region.Poland => ("pl", "+48 ## ### ## ##",
                [
                    f => $"{f.Address.StreetName()} {f.Address.BuildingNumber()}, {f.Address.ZipCode()} {f.Address.City()}",
                    f => $"{f.Address.StreetName()} {f.Address.BuildingNumber()}/{f.Random.Number(1, 100)}, {f.Address.ZipCode()} {f.Address.City()}",
                    f => $"{f.Address.StreetName()} {f.Address.BuildingNumber()}, m. {f.Random.Number(1, 100)}, {f.Address.ZipCode()} {f.Address.City()}",
                    f => $"{f.Address.City()}, {f.Address.StreetName()} {f.Address.BuildingNumber()}, {f.Address.ZipCode()} {f.Address.State()}"
                ]),
                Region.Ukraine => ("uk", "+380 ## ### ## ##",
                [
                    f => $"{f.Address.StreetName()}, {f.Address.BuildingNumber()}, {f.Address.City()}, {f.Address.StateAbbr()} обл., {f.Address.ZipCode()}",
                    f => $"{f.Address.StreetName()}, буд. {f.Address.BuildingNumber()}, кв. {f.Random.Number(1, 100)}, м. {f.Address.City()}, {f.Address.StateAbbr()} обл., {f.Address.ZipCode()}",
                    f => $"{f.Address.City()}, {f.Address.StreetName()} {f.Address.BuildingNumber()}, {f.Address.ZipCode()}",
                    f => $"{f.Address.StateAbbr()} обл., м. {f.Address.City()}, {f.Address.StreetName()}, {f.Address.BuildingNumber()}"
                ])
            };
        }


        private static string GenerateAddress(Faker f, Func<Faker, string>[] addressFormats)
        {
            var format = f.PickRandom(addressFormats);
            return format(f);
        }

        private static User IntroduceErrors(User user, double errorCount, int seed, Region region)
        {
            var random = new Random(seed);
            var actualErrorCount = Math.Floor(errorCount) + (random.NextDouble() < (errorCount % 1) ? 1 : 0);

            for (int i = 0; i < actualErrorCount; i++)
            {
                switch (random.Next(3))
                {
                    case 0:
                        user.FullName = ApplyError(user.FullName, random, GetAlphabetForRegion(region));
                        break;
                    case 1:
                        user.Address = ApplyError(user.Address, random, GetAlphabetForRegion(region));
                        break;
                    default:
                        user.Phone = ApplyError(user.Phone, random, DIGITS);
                        break;
                }
            }

            return user;
        }

        private static string ApplyError(string input, Random random, string characterSet)
        {
            return random.Next(3) switch
            {
                0 => DeleteRandomCharacter(input, random),
                1 => AddRandomCharacter(input, random, characterSet),
                _ => SwapRandomCharacter(input, random),
            };
        }

        private static string DeleteRandomCharacter(string value, Random random)
        {
            if (string.IsNullOrEmpty(value))
            {
                var index = random.Next(value.Length);
                value = value.Remove(index, 1);
            }
            return value;
        }

        private static string AddRandomCharacter(string value, Random random, string characterSet)
        {
            var index = random.Next(value.Length + 1);
            var newChar = random.Next(0, characterSet.Length);
            return value.Insert(index, characterSet[newChar].ToString());
        }

        private static string SwapRandomCharacter(string value, Random random)
        {
            if (value.Length >= 2)
            {
                var index = random.Next(value.Length - 1);
                var chars = value.ToCharArray();
                (chars[index], chars[index + 1]) = (chars[index + 1], chars[index]);
                return new string(chars);
            }
            return value;
        }

        private static int CombineSeedWithPage(string seed, int page, int pageSize) => seed.GetHashCode() + page * pageSize;

        private static string GetAlphabetForRegion(Region region) => region switch
        {
            Region.USA => USA_ALPHABET,
            Region.Poland => POLAND_ALPHABET,
            Region.Ukraine => UKRAINIAN_ALPHABET
        };
    }
}