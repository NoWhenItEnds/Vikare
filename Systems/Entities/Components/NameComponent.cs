using System;
using Vikare.Utilities.Extensions;
using static Vikare.Utilities.Extensions.CsvExtensions;

namespace Vikare.Entities.Components
{
    /// <summary> A person's full name with reference to their personal and clan names. </summary>
    /// <remarks Uses: https://github.com/sigpwned/popular-names-by-country-dataset </remarks>
    public partial class NameComponent : EntityComponent
    {
        /// <summary> An entity's common, personal name. </summary>
        public GivenName FirstName { get; init; } = GivenName.Empty;

        /// <summary> An entity's family / clan name. </summary>
        public Surname LastName { get; init; } = Surname.Empty;


        /// <summary> In memory reference to all the first names. </summary>
        private static GivenName[] FIRST_NAMES = CsvExtensions.LoadData<GivenName>("res://Content/Data/FirstNames.csv");

        /// <summary> In memory reference to all the first names. </summary>
        private static Surname[] LAST_NAMES = CsvExtensions.LoadData<Surname>("res://Content/Data/LastNames.csv");


        /// <inheritdoc/>
        public override String ToString() => $"{FirstName.Male} {LastName.Romanised}";


        /// <summary> An empty, default name. </summary>
        public static NameComponent Empty => new NameComponent();


        /// <summary> Generate a random name. </summary>
        /// <returns> The generated name. </returns>
        public static NameComponent Random()
        {
            return new NameComponent
            {
                FirstName = FIRST_NAMES.GetRandomElement() ?? GivenName.Empty,
                LastName = LAST_NAMES.GetRandomElement() ?? Surname.Empty
            };
        }

        /// <inheritdoc/>
        public override Int32 GetHashCode() => HashCode.Combine(FirstName, LastName);


        /// <inheritdoc/>
        public Boolean Equals(NameComponent? other) => other != null ? FirstName.Equals(other.FirstName) && LastName.Equals(other.LastName) : false;
    }


    /// <summary> An entity's common, personal name. </summary>
    public record GivenName : IParseable<GivenName> // TODO - Make like Male names and Female equivalent.
    {
        /// <summary> The name's male-equivalent. </summary>
        public String Male { get; init; } = String.Empty;

        /// <summary> The name's female-equivalent. </summary>
        public String Female { get; init; } = String.Empty;


        /// <summary> An empty, default name. </summary>
        public static GivenName Empty => new GivenName();


        /// <inheritdoc/>
        public static GivenName Parse(String[] header, String[] data)
        {
            Int32 maleIndex = header.IndexOf("Male Name");
            Int32 femaleIndex = header.IndexOf("Female Name");

            String nameName = maleIndex != -1 ? data[maleIndex] : String.Empty;
            String femaleName = femaleIndex != -1 ? data[femaleIndex] : String.Empty;

            return new GivenName
            {
                Male = nameName,
                Female = femaleName
            };
        }
    }


    /// <summary> An entity's family / clan name. </summary>
    public record Surname : IParseable<Surname> // TODO - Make resource editable in inspector. Add ties to clan details.
    {
        /// <summary> The common Alpha-2 designation of the name's country of origin. </summary>
        public String CountryISO { get; init; } = String.Empty;

        /// <summary> The name as it appears in its local language, potentially using non-Latin characters. </summary>
        public String Localised { get; init; } = String.Empty;

        /// <summary> The name as it appears in in English, should only use Latin characters. </summary>
        public String Romanised { get; init; } = String.Empty;


        /// <summary> An empty, default name. </summary>
        public static Surname Empty => new Surname();


        /// <inheritdoc/>
        public static Surname Parse(String[] header, String[] data)
        {
            Int32 countryIndex = header.IndexOf("Country");
            Int32 localisedIndex = header.IndexOf("Localized Name");
            Int32 romanisedIndex = header.IndexOf("Romanized Name");

            return new Surname
            {
                CountryISO = countryIndex != -1 ? data[countryIndex] : String.Empty,
                Localised = localisedIndex != -1 ? data[localisedIndex] : String.Empty,
                Romanised = romanisedIndex != -1 ? data[romanisedIndex] : String.Empty
            };
        }
    }
}
