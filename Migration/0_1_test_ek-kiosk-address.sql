--
-- Скрипт сгенерирован Devart dbForge Studio 2019 for SQL Server, Версия 5.8.127.0
-- Домашняя страница продукта: http://www.devart.com/ru/dbforge/sql/studio
-- Дата скрипта: 2020-11-10 03:46:13
-- Версия сервера: 12.0.2000.8
--

SET DATEFORMAT ymd
SET ARITHABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
SET NUMERIC_ROUNDABORT, IMPLICIT_TRANSACTIONS, XACT_ABORT OFF
GO

SET IDENTITY_INSERT dbo.Addresses ON
GO
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (23, N'ул. Предславинская 30', NULL, N'Киев', NULL, NULL, 233)
GO
SET IDENTITY_INSERT dbo.Addresses OFF
GO