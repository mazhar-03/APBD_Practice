INSERT INTO Country (Name)
VALUES
    ('United States'),
    ('Canada'),
    ('Germany'),
    ('France');

INSERT INTO Currency (Name, Rate)
VALUES
    ('USD', 1.000),
    ('CAD', 0.740),
    ('EUR', 1.090),
    ('GBP', 1.250);

INSERT INTO Currency_Country (Country_Id, Currency_Id)
VALUES
    ((SELECT Id FROM Country WHERE Name = 'United States'), (SELECT Id FROM Currency WHERE Name = 'USD')),
    ((SELECT Id FROM Country WHERE Name = 'Canada'), (SELECT Id FROM Currency WHERE Name = 'CAD')),
    ((SELECT Id FROM Country WHERE Name = 'Germany'), (SELECT Id FROM Currency WHERE Name = 'EUR')),
    ((SELECT Id FROM Country WHERE Name = 'France'), (SELECT Id FROM Currency WHERE Name = 'EUR'));
