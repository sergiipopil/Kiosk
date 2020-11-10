--
-- Скрипт сгенерирован Devart dbForge Studio 2019 for SQL Server, Версия 5.8.127.0
-- Домашняя страница продукта: http://www.devart.com/ru/dbforge/sql/studio
-- Дата скрипта: 2020-11-10 03:46:37
-- Версия сервера: 12.0.2000.8
--

SET DATEFORMAT ymd
SET ARITHABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
SET NUMERIC_ROUNDABORT, IMPLICIT_TRANSACTIONS, XACT_ABORT OFF
GO

SET IDENTITY_INSERT dbo.Kiosks ON
GO
INSERT dbo.Kiosks(Id, CustomerId, Status, SerialKey, AssignedKioskVersion, AddressId, CommaSeparatedLanguageCodes, AdminModePassword, CommunicationComments, WorkflowComponentConfigurationsJson, LastPingedOnUtc, ApplicationType) 
VALUES (23, 1, 1, N'YJJL8FA3ONX67IQZMM4KAOWR55DPE89J2BNYGPNTF45ZTCLOPONZQQAJX1QCBUZPOOD642T68LEGT682UZ9ZVJ3MBEAO8LRVEPLF8OUX5R0EL3Z12YSMU94FV14Z1F7X', N'1.2.14', 23, N'ru', N'Ek#1', NULL, N'[{"componentRole":"EK","componentName":"EkApplication","settings":{"IsAdvertisementDisplayDisabled":false}}]', '2020-11-09 20:23:44.6347429', 2)
GO
SET IDENTITY_INSERT dbo.Kiosks OFF
GO