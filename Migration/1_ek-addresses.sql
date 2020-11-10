--
-- Скрипт сгенерирован Devart dbForge Studio 2019 for SQL Server, Версия 5.8.127.0
-- Домашняя страница продукта: http://www.devart.com/ru/dbforge/sql/studio
-- Дата скрипта: 2020-11-10 02:40:02
-- Версия сервера: 12.0.2000.8
--

SET DATEFORMAT ymd
SET ARITHABORT, ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER, ANSI_NULLS, NOCOUNT ON
SET NUMERIC_ROUNDABORT, IMPLICIT_TRANSACTIONS, XACT_ABORT OFF
GO

SET IDENTITY_INSERT dbo.Addresses ON
GO
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (25, N'ул. Матросова 19', NULL, N'Мукачево', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (26, N'ул. Берковецкая 6, Ашан', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (27, N'ул.Грушевского 13', NULL, N'Великий Бычков', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (28, N'ул. Лавкивськая 1е', NULL, N'Мукачево', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (29, N'ул. Ивана Франко 135е', NULL, N'Виноградов', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (30, N'Московський проспект, 195', NULL, N'Харків', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (31, N'ул. Великая Окружная 4б', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (32, N'Овидиопольская дорога, 3а', NULL, N'Одесса', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (33, N'Новониколаевская дорога 1, Суворовский, Таврия В', NULL, N'Одесса', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (34, N'ул. Гагарина 38', NULL, N'Ужгород', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (35, N'ул. 1 мая 70А, ТЦ Европейский', NULL, N'Гайсин', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (36, N'ул. Чернобыльская 16/80', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (37, N'ул. Семьи Cосниных 17', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (38, N'ул. Гагарина 111а', NULL, N'Иршава', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (39, N'ул. Мукачевская 190/2', NULL, N'Берегово', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (40, N'ул. Петра Калнышевского 2', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (41, N'ул. Предславинская 30', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (42, N'ул. Глушкова 13б', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (43, N'ул. Борканюка 42', NULL, N'Свалява', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (44, N'ул. Гната Хоткевича 1', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (45, N'вул. Білопільський Шлях 20/2', NULL, N'Суми', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (46, N'проспект Московський, 138Г', NULL, N'Харків', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (47, N'пр. Курський 135', NULL, N'Суми', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (48, N'ул. Химиков 29', NULL, N'Южный', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (49, N'вул. Грушевського, 8', NULL, N'Глеваха', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (50, N'Київське шосе, 1', NULL, N'Одеса', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (51, N'Запорожское шоссе, 2, ТРЦ "ПортCity"', NULL, N'Мариуполь', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (52, N'вулиця Безлюдівська, 1', NULL, N'Комунар', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (53, N'ул. Героев Сталинграда 8', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (54, N'вул. Садовського, 1', NULL, N'Бузова', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (55, N'ул. Лазурная 17', NULL, N'Николаев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (56, N'ул. Борщаговская 154', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (57, N'вулиця Шосейна, 180', NULL, N'Підгородне', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (58, N'пр. Богоявленский 234в', NULL, N'Николаев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (59, N'ул. Люстдорфская дорога 140/1', NULL, N'Одесса', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (60, N'вулиця Залютинська, 3', NULL, N'Харків', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (61, N'пр. Мира 35', NULL, N'Черноморск', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (62, N'вул. Чорноморського Козацтва, 96', NULL, N'Одеса', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (63, N'вул. Київська 23', NULL, N'Вишневе', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (64, N'ул. Предславинская 30', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (65, N'пр. Карабелов 20/3', NULL, N'Николаев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (66, N'26 км автошляху Київ-Луганськ', NULL, N'Підгірці', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (67, N'вул. Чигоріна, 12', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (68, N'ул. Героев Крут, 29', NULL, N'Сумы', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (69, N'пр. Курський 135', NULL, N'Суми', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (70, N'ул. Ярославская 55', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (71, N'вул. Комарова, 70', NULL, N'Мила', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (72, N'Okruzhna Rd', NULL, N'Харків', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (73, N'пр. Суворова 181', NULL, N'Измаил', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (74, N'просп. Генерала Ватутина 15', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (75, N'ул. Предславинская 30', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (76, N'ул. Предславинская 30', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (77, N'вул. Янтарна, 1', NULL, N'Віта-Поштова', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (78, N'ул. Предславинская 30', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (79, N'ул. Предславинская 30', NULL, N'Киев', NULL, NULL, 233)
INSERT dbo.Addresses(Id, AddressLine1, AddressLine2, City, ZipCode, StateId, CountryId) VALUES (80, N'вул. Балківська, 1', NULL, N'Одеса', NULL, NULL, 233)
GO
SET IDENTITY_INSERT dbo.Addresses OFF
GO