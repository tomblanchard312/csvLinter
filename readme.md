# csvLinter

The `csvLinter` is a .NET Core Web API project designed to validate CSV files against a specified schema. This tool ensures that CSV files conform to expected formats and data types.
You can add your own datatypes as needed, via the schemas folder in JSON format. This contains a few simple examples.

## Features

- Validate CSV files for correct number of columns and specific data types.
- Dynamic schema definitions using a Json to map column names, types and values.
- Easy integration into other systems or as a standalone service.

## Prerequisites

Before you begin, ensure you have the following installed:
- [.NET 6 SDK](https://dotnet.microsoft.com/download) or later
- [Visual Studio](https://visualstudio.microsoft.com/) (optional, but recommended for ease of use)

## Project Setup

1. **Clone the Repository**
   Clone this repository to your local machine using Git:
```
   git clone https://github.com/tomblanchard312/csvLinter.git
   cd csvLinter
```

Navigate to the API Project
Change directory to the API project where the .csproj file is located:

```
cd csvLinter.Api
```
Restore Dependencies
Run the following command to restore all the necessary .NET packages:
```
dotnet restore
```
Build the Project
Compile the project to check for any errors:

Running the Application
To run the API locally, use the following command from the directory containing the csvSQLLinter.Api.csproj file:

```
dotnet run
```
The API will start on [http://localhost:7014](https://localhost:7014/) by default. You can access the API endpoints using a browser or tools like Postman to submit CSV files for linting.

Using the API
Endpoint: /csvlint
Method: POST
Body: Multipart/form-data with the file upload

Example using cURL:
```
curl -X POST -F "file=@path_to_your_file.csv" https://localhost:7014/csvlint
```
Testing
Included in the project are sample CSV files like Sample_Employee_Data_Good.csv with dummy data for testing. Use this file to test the CSV linting capabilities of the API.

Contributing
Contributions to the csvLinter are welcome. Please fork the repository and submit pull requests to contribute.

License
This project is licensed under the [Apache 2.0 License](LICENSE).


