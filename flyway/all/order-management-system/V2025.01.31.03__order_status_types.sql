SET search_path to oms;

CREATE TABLE order_status_types (
    code VARCHAR(30) PRIMARY KEY,
    description VARCHAR(100) NOT NULL,
    create_date TIMESTAMP NOT NULL DEFAULT NOW(),
    update_date TIMESTAMP NOT NULL DEFAULT NOW()
);