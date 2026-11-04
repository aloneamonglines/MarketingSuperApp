CREATE ROLE identity_runtime
    LOGIN
    PASSWORD 'identity_runtime_dev'
    NOSUPERUSER
    NOCREATEDB
    NOCREATEROLE
    NOINHERIT
    NOBYPASSRLS;

GRANT CONNECT ON DATABASE identity_db TO identity_runtime;

CREATE SCHEMA identity AUTHORIZATION identity_migrator;
GRANT USAGE ON SCHEMA identity TO identity_runtime;

ALTER DEFAULT PRIVILEGES FOR ROLE identity_migrator IN SCHEMA identity
    GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO identity_runtime;

ALTER DEFAULT PRIVILEGES FOR ROLE identity_migrator IN SCHEMA identity
    GRANT USAGE, SELECT ON SEQUENCES TO identity_runtime;
