# G9 API (Backend)

[![License](https://img.shields.io/github/license/VMVT-DevHub/g9-app)](https://github.com/VMVT-DevHub/g9-app/blob/main/LICENSE)
[![GitHub issues](https://img.shields.io/github/issues/VMVT-DevHub/g9-app)](https://github.com/VMVT-DevHub/g9-app/issues)
[![GitHub stars](https://img.shields.io/github/stars/VMVT-DevHub/g9-app)](https://github.com/VMVT-DevHub/g9-app/stargazers)

This repository contains the source code and documentation for the G9 Backend API, developed by the Valstybinė maisto ir veterinarijos tarnyba

## About the Project

The G9 Backend API is designed to provide information and functionalities related to drinkable water monitoring data.

This is a client application that utilizes
the [G9-UI](https://github.com/VMVT-DevHub/g9-app).

Key features of the WEB include:

- Providing monitoring data of drinkable water
- Providing declarations of drinkable water monitoring data

## Getting Started

To get started with the G9 API, follow the instructions below.

### Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/VMVT-DevHub/G9-API.git
   ```

2. Install the required dependencies:

   ```bash
   cd G9-API
   dotnet restore
   ```
3. Set up the PostgreSQL database and add connection details to appsettings.json file. 

### Usage

1. Start the WEB server:

   ```bash
   dotnet run
   ```

2. The API details will be available at `http://localhost:5501/swagger/`.

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a
pull request. For more information, see the [contribution guidelines](./CONTRIBUTING.md).

## License

This project is licensed under the [MIT License](./LICENSE).
