# HttpTool

A C# command-line utility for testing website health, redirects, HTML validation, and common SEO issues.

## Installation

### Download

If you are just interested in using the program, you can download the latest release for your OS from the Releases page.


### Build from source

git clone https://github.com/jamesjohnmcguire/HttpTool

Refer to DevelopmentTools Build Scripts for specific build examples.

## Usage

This is a command line program to be run from the command line or terminal.

HttpTool performs a collection of automated website diagnostics, including redirect validation, missing image detection, HTML validation, and empty page detection.

### Command line usage:


HttpTool \<command\> \<url\>

| Commands:    |                               |
| ------------ | ----------------------------- |
| agilitypack  | Run agility pack tests        |
| empty        | Run empty page tests          |
| enhanced     | Run all enhanced tests        |
| images       | Run missing images tests      |
| redirects    | Run redirect tests            |
| standard     | Run standard tests (default)  |
| testall      | Run all tests                 |
| validate     | Run w3c HTML validation tests |
| help         | Display this information      |

### Examples
HttpTool standard https://example.com

HttpTool testall https://example.com

HttpTool validate https://example.com

## Contributing

If you have found a bug or have a suggestion that would make this better, please fork this repository and create a pull request. You can also simply open an issue with the tag "bug" or "enhancement".

### Process:

1. Fork the Project
2. Create your Bug / Feature Branch (`git checkout -b feature/amazing-feature`)
3. Commit your Changes (`git commit -m 'Add some amazing feature'`)
4. Push to the Branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Coding style
Please match the current coding style.  Most notably:
1. One operation per line
2. Use complete English words in variable and method names
3. Attempt to declare variable and method names in a self-documenting manner
4. Add unit tests


## License

Distributed under the MIT License. See `LICENSE` for more information.

## Contact

James John McGuire - jamesjohnmcguire@gmail.com [LinkedIn](https://twitter.com/https://www.linkedin.com/in/jamesjohnmcguire) -  [GitHub](https://github.com/jamesjohnmcguire)

Project Link: [HttpTool](https://github.com/jamesjohnmcguire/HttpTool)
