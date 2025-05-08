CREATE TABLE Country (
                         Id INT IDENTITY(1,1) PRIMARY KEY,
                         Name VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE Currency (
                          Id INT IDENTITY(1,1) PRIMARY KEY,
                          Name VARCHAR(100) NOT NULL UNIQUE,
                          Rate FLOAT(3) NOT NULL
);

CREATE TABLE Currency_Country (
                                  Country_Id INT,
                                  Currency_Id INT,
                                  PRIMARY KEY (Country_Id, Currency_Id),
                                  FOREIGN KEY (Country_Id) REFERENCES Country(Id),
                                  FOREIGN KEY (Currency_Id) REFERENCES Currency(Id)
);
