SET search_path TO oms;

INSERT INTO customers (id, name, email)
VALUES 
    ('4638e60b-e741-4e3b-b7f3-37419a5c8ad6', 'John', 'john@xyz.com'),
    ('e7834256-2430-4c38-9987-f72d0deb85cb', 'Tom', 'tom@xyz.com')
ON CONFLICT (id)
DO UPDATE SET
    name = EXCLUDED.name, email = EXCLUDED.email;
