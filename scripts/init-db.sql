-- Initialize the ticketing platform database with schemas

-- Create schemas for each module
CREATE SCHEMA IF NOT EXISTS users;
CREATE SCHEMA IF NOT EXISTS events;
CREATE SCHEMA IF NOT EXISTS teams;
CREATE SCHEMA IF NOT EXISTS tickets;
CREATE SCHEMA IF NOT EXISTS payments;

-- Grant privileges to the application user
GRANT USAGE ON SCHEMA users TO ticketing_admin;
GRANT USAGE ON SCHEMA events TO ticketing_admin;
GRANT USAGE ON SCHEMA teams TO ticketing_admin;
GRANT USAGE ON SCHEMA tickets TO ticketing_admin;
GRANT USAGE ON SCHEMA payments TO ticketing_admin;

GRANT ALL PRIVILEGES ON SCHEMA users TO ticketing_admin;
GRANT ALL PRIVILEGES ON SCHEMA events TO ticketing_admin;
GRANT ALL PRIVILEGES ON SCHEMA teams TO ticketing_admin;
GRANT ALL PRIVILEGES ON SCHEMA tickets TO ticketing_admin;
GRANT ALL PRIVILEGES ON SCHEMA payments TO ticketing_admin;

-- Grant privileges on all tables in each schema
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA users TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA events TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA teams TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA tickets TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA payments TO ticketing_admin;

-- Grant privileges on all sequences in each schema
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA users TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA events TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA teams TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA tickets TO ticketing_admin;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA payments TO ticketing_admin;

-- Set default privileges for future objects
ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT ALL ON TABLES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA events GRANT ALL ON TABLES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA teams GRANT ALL ON TABLES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA tickets GRANT ALL ON TABLES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA payments GRANT ALL ON TABLES TO ticketing_admin;

ALTER DEFAULT PRIVILEGES IN SCHEMA users GRANT ALL ON SEQUENCES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA events GRANT ALL ON SEQUENCES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA teams GRANT ALL ON SEQUENCES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA tickets GRANT ALL ON SEQUENCES TO ticketing_admin;
ALTER DEFAULT PRIVILEGES IN SCHEMA payments GRANT ALL ON SEQUENCES TO ticketing_admin;

-- Enable UUID extension for all schemas
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Log the initialization
INSERT INTO pg_catalog.pg_stat_statements_info VALUES ('Database schemas initialized for Ticketing Platform') 
ON CONFLICT DO NOTHING;

-- Create a simple logging table (optional)
CREATE TABLE IF NOT EXISTS public.deployment_log (
    id SERIAL PRIMARY KEY,
    message TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);

INSERT INTO public.deployment_log (message) VALUES ('Database schemas initialized successfully');
