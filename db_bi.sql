-- Используем целевую базу данных
USE db_bi;

SET foreign_key_checks = 0;

DROP TABLE IF EXISTS Visualization;
DROP TABLE IF EXISTS Report;
DROP TABLE IF EXISTS DataSource;
DROP TABLE IF EXISTS User;
DROP TABLE IF EXISTS Organization;

SET foreign_key_checks = 1;

-- Создание таблиц

-- 1. Таблица Organization (Организация)
CREATE TABLE Organization (
    organization_id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    industry VARCHAR(50),
    registration_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    contact_email VARCHAR(100) NOT NULL,
    subscription_plan ENUM('free', 'basic', 'premium') DEFAULT 'free'
);

-- 2. Таблица User (Пользователь)
CREATE TABLE User (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    organization_id INT NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('admin', 'analyst', 'manager') NOT NULL,
    last_login DATETIME,
    FOREIGN KEY (organization_id) REFERENCES Organization(organization_id)
);

-- 3. Таблица DataSource (Источник данных)
CREATE TABLE DataSource (
    source_id INT AUTO_INCREMENT PRIMARY KEY,
    organization_id INT NOT NULL,
    type ENUM('API', 'CSV', 'Database', 'Manual') NOT NULL,
    connection_string VARCHAR(255),
    last_sync DATETIME,
    sync_frequency ENUM('daily', 'weekly', 'monthly'),
    FOREIGN KEY (organization_id) REFERENCES Organization(organization_id)
);

-- 4. Таблица Report (Отчет)
CREATE TABLE Report (
    report_id INT AUTO_INCREMENT PRIMARY KEY,
    organization_id INT NOT NULL,
    title VARCHAR(100) NOT NULL,
    description TEXT,
    creation_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    last_modified DATETIME ON UPDATE CURRENT_TIMESTAMP,
    is_public BOOLEAN DEFAULT FALSE,
    FOREIGN KEY (organization_id) REFERENCES Organization(organization_id)
);

-- 5. Таблица Visualization (Визуализация)
CREATE TABLE Visualization (
    visualization_id INT AUTO_INCREMENT PRIMARY KEY,
    report_id INT NOT NULL,
    type ENUM('chart', 'table', 'diagram', 'kpi') NOT NULL,
    data_query TEXT,
    settings JSON,
    FOREIGN KEY (report_id) REFERENCES Report(report_id)
);

-- Заполнение таблиц

-- 1. Добавляем три организации
INSERT INTO Organization (name, industry, contact_email, subscription_plan)
VALUES 
    ('ООО "Ромашка"', 'Розничная торговля', 'romashka@example.com', 'premium'),
    ('ИП Иванов', 'Услуги', 'ivanov@example.com', 'basic'),
    ('АО "ТехноКом"', 'IT', 'tech@example.com', 'free');

-- 2. Добавляем пользователей для каждой организации
INSERT INTO User (organization_id, email, password_hash, role, last_login)
VALUES
    -- Пользователи для ООО "Ромашка" (organization_id = 1)
    (1, 'admin@romashka.com', SHA2('admin123', 256), 'admin', '2024-01-15 10:00:00'),
    (1, 'analyst@romashka.com', SHA2('analyst123', 256), 'analyst', '2024-01-16 11:30:00'),
    
    -- Пользователь для ИП Иванов (organization_id = 2)
    (2, 'ivanov@example.com', SHA2('ivanov123', 256), 'admin', '2024-01-10 09:15:00'),
    
    -- Пользователи для АО "ТехноКом" (organization_id = 3)
    (3, 'manager@tech.com', SHA2('manager123', 256), 'manager', '2024-01-05 14:20:00'),
    (3, 'dev@tech.com', SHA2('dev123', 256), 'analyst', NULL);

-- 3. Добавляем источники данных
INSERT INTO DataSource (organization_id, type, connection_string, last_sync, sync_frequency)
VALUES
    -- Для ООО "Ромашка"
    (1, 'Database', 'mysql://user:pass@localhost/romashka_db', '2024-01-20 08:00:00', 'daily'),
    (1, 'CSV', '/data/import/sales.csv', '2024-01-19 17:30:00', 'weekly'),
    
    -- Для ИП Иванов
    (2, 'API', 'https://api.ivanov.com/data', '2024-01-18 12:00:00', 'monthly'),
    
    -- Для АО "ТехноКом"
    (3, 'Manual', NULL, NULL, NULL);

-- 4. Добавляем отчеты
INSERT INTO Report (organization_id, title, description, is_public)
VALUES
    -- Отчеты ООО "Ромашка"
    (1, 'Продажи за январь', 'Анализ продаж за январь 2024 года', TRUE),
    (1, 'Финансовый отчет', 'Доходы и расходы за Q1', FALSE),
    
    -- Отчет ИП Иванов
    (2, 'Клиентская база', 'Список клиентов и их активность', TRUE),
    
    -- Отчеты АО "ТехноКом"
    (3, 'Разработка ПО', 'Статус проектов разработки', FALSE),
    (3, 'Анализ сервера', 'Нагрузка на сервера за месяц', TRUE);

-- 5. Добавляем визуализации для отчетов
INSERT INTO Visualization (report_id, type, data_query, settings)
VALUES
    -- Визуализации для отчета "Продажи за январь" (report_id = 1)
    (1, 'chart', 'SELECT date, SUM(amount) FROM sales GROUP BY date', '{"type": "bar", "title": "Продажи по дням"}'),
    (1, 'kpi', 'SELECT COUNT(DISTINCT customer_id) FROM sales', '{"title": "Уникальные клиенты"}'),
    
    -- Визуализация для отчета "Клиентская база" (report_id = 3)
    (3, 'table', 'SELECT name, email, last_purchase FROM clients', '{"columns": ["Имя", "Email", "Последний заказ"]}'),
    
    -- Визуализация для отчета "Разработка ПО" (report_id = 4)
    (4, 'diagram', 'SELECT status, COUNT(*) FROM tasks GROUP BY status', '{"type": "pie", "title": "Статусы задач"}');
    
-- Запросы к базе данных

-- 9. Представление: активные источники данных
CREATE VIEW ActiveDataSources AS
SELECT organization_id, type, last_sync
FROM DataSource
WHERE last_sync IS NOT NULL;


-- 18. Визуализации к публичным отчетам
SELECT v.*
FROM Visualization v
JOIN Report r ON v.report_id = r.report_id
WHERE r.is_public = TRUE;
