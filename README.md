# 국세청 전자세금계산서 연동 서비스 엔진 

처리 하는 엔진 서비스들은 총 6가지로 구성 됩니다.

> collector: 전자 세금계산서를 작성 후 데이터베이스에 저장 하게 됩니다. 엑셀 파일을 업로드 하거나, ERP 시스템과 연동하여 다량의 세금계산서를 처리 합니다.

> signer: 매출자의 공인인증서로 서명하여, 발송 전 단계의 암호화된 전자세금계산서를 작성 합니다.

> reporter: 국세청에 전자세금계산서를 발송 합니다

> reponsor: 국세청으로 부터 처리 결과를 수신 받아서 내부 데이터베이스 저장 합니다.

> mailer: ASP 사업자와 매입자에게 전자세금계산서를 메일로 발송 합니다.

> provider: 타 ASP 사업자가 보내온 메일을 파싱하여 매입 전자세금계산서를 내부 데이터베이스에 저장 합니다.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisities

What things you need to install the software and how to install them

```
Give examples
```

### Installing

A step by step series of examples that tell you have to get a development env running

Stay what the step will be

```
Give the example
```

And repeat

```
until finished
```

End with an example of getting some data out of the system or using it for a little demo

## Running the tests

Explain how to run the automated tests for this system

### Break down into end to end tests

Explain what these tests test and why

```
Give an example
```

### And coding style tests

Explain what these tests test and why

```
Give an example
```

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* Dropwizard - Bla bla bla
* Maven - Maybe
* Atom - ergaerga

## Contributing

Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **SeongAhn Lee** - *Initial work* - [JACK-LEE](https://github.com/lisa3907)

See also the list of [contributors](https://github.com/open-etaxbill/etaxbill-certifier/graphs/contributors) who participated in this project.

## License

This project is licensed under the GPL License - see the [LICENSE.md](LICENSE.md) file for details

## Acknowledgments

* Hat tip to anyone who's code was used
* Inspiration
* etc
