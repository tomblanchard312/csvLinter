using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace csvLinter.Api.Helpers
{
    public static class CsvValidationHelper
    {
        public class ColumnSchema
        {
            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("maxLength")]
            public int MaxLength { get; set; }

            [JsonPropertyName("values")]
            public List<string> Values { get; set; }
        }

        public static readonly Dictionary<string, Dictionary<string, ColumnSchema>> Schemas = new Dictionary<string, Dictionary<string, ColumnSchema>>();

        public static Dictionary<string, ColumnSchema> GetSchema(string schemaType)
        {
            if (!Schemas.ContainsKey(schemaType))
            {
                string schemaFilePath = Path.Combine(Directory.GetCurrentDirectory(), "schemas", $"{schemaType}.json");
                if (File.Exists(schemaFilePath))
                {
                    string jsonString = File.ReadAllText(schemaFilePath);
                    var schema = JsonSerializer.Deserialize<Dictionary<string, ColumnSchema>>(jsonString);
                    Schemas[schemaType] = schema;
                }
                else
                {
                    throw new FileNotFoundException($"Schema file not found for type: {schemaType}");
                }
            }
            return Schemas[schemaType];
        }

        public static void LoadSchemas()
        {
            // existing code...
            if (Schemas != null)
            {
                foreach (var schema in Schemas)
                {
                    Console.WriteLine($"Schema Type: {schema.Key}");
                    foreach (var field in schema.Value)
                    {
                        Console.WriteLine($"Field: {field.Key}, Type: {field.Value.Type}, MaxLength: {field.Value.MaxLength}");
                        if (field.Value.Values != null)
                            Console.WriteLine($"Enum Values: {string.Join(", ", field.Value.Values)}");
                    }
                }
            }
        }
        public static bool ValidateField(string schemaType, string fieldName, string value, ColumnSchema columnSchema, out string errorMessage)
        {
            errorMessage = null;

            if (columnSchema == null)
            {
                throw new ArgumentException("ColumnSchema must not be null.");
            }

            // Custom validations based on column name
            switch (fieldName.ToLower())
            {
                case "birthdate":
                case "dateofbirth":
                case "date":
                    var dateFormats = new List<string>
                    {
                        "dd-MMM-yyyy",
                        "dd-MMM-yy",
                        "yyyy-MM-dd",
                        "yyyy/MM/dd",
                        "dd/MM/yyyy",
                        "M/d/yyyy h:mm:ss tt",
                        "M/d/yyyy"
                    };
                    if (!IsValidDate(value, dateFormats))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid date. Expected formats: {string.Join(", ", dateFormats)}.";
                        return false;
                    }
                    if (fieldName.ToLower() == "birthdate" || fieldName.ToLower() == "dateofbirth")
                    {
                        if (!IsLogicalBirthDate(DateTime.Parse(value)))
                        {
                            errorMessage = $"{fieldName} value '{value}' is not a logical birth date.";
                            return false;
                        }
                    }
                    break;
                case "email":
                    if (!IsValidEmail(value))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid email address.";
                        return false;
                    }
                    break;
                case "phonenumber":
                    if (!IsValidPhoneNumber(value))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid phone number.";
                        return false;
                    }
                    break;
                case "tsaprecheck":
                case "knowntravelernumber":
                case "ktn":
                    if (!IsValidKnownTravelerNumber(value))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid Known Traveler Number. It must be a 9-digit number.";
                        return false;
                    }
                    break;
                case "redressnumber":
                    if (!IsValidRedressNumber(value))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid Redress Number. It must be a 7-digit number.";
                        return false;
                    }
                    break;
            }
            //rest of validation
            switch (columnSchema.Type?.ToLower())
            {
                case "int":
                    if (!int.TryParse(value, out _))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid integer.";
                        return false;
                    }
                    return true;
                case "string":
                    if (value.Length > columnSchema.MaxLength)
                    {
                        errorMessage = $"{fieldName} value '{value}' exceeds the maximum length of {columnSchema.MaxLength}.";
                        return false;
                    }
                    return true;
                case "date":
                    if (!DateTime.TryParse(value, out _))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid date.";
                        return false;
                    }
                    return true;
                case "enum":
                    if (columnSchema.Values == null || !columnSchema.Values.Contains(value))
                    {
                        errorMessage = $"{fieldName} value '{value}' is not a valid value. Expected values: {string.Join(", ", columnSchema.Values ?? new List<string>())}.";
                        return false;
                    }
                    return true;
                default:
                    throw new NotSupportedException($"Unsupported type {columnSchema.Type} in schema.");
            }
        }
        private static bool IsValidDate(string date, List<string> formats)
        {
            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                    return true;
            }
            return false;
        }

        private static bool IsLogicalBirthDate(DateTime date)
        {
            if (date > DateTime.Now || date < DateTime.Now.AddYears(-120))
            {
                return false;
            }
            return true;
        }
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Simplified regular expression for basic email validation
            string pattern = @"^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$";
            return Regex.IsMatch(email, pattern);
        }
        public static bool IsValidPhoneNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                return false;

            // Regex pattern to match North American numbers, international numbers with + prefix, extensions, and 800 numbers.
            string pattern = @"^\+?(\d{1,3})?[-. ]?\(?\d{3}\)?[-. ]?\d{3}[-. ]?\d{4}(\s*(x|ext)\s*\d{1,6})?$";
            return Regex.IsMatch(number, pattern);
        }
        //validate KTN length
        public static bool IsValidKnownTravelerNumber(string ktn)
        {
            return long.TryParse(ktn, out long _number) && ktn.Length == 9;
        }
        //validate TSA Redress length
        public static bool IsValidRedressNumber(string redressNumber)
        {
            return int.TryParse(redressNumber, out int _number) && redressNumber.Length == 7;
        }

    }

}
