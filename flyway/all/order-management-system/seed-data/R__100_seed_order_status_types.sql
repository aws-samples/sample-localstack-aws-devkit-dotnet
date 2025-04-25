SET search_path to oms;

INSERT INTO oms.order_status_types (code, description)
VALUES
    ('New', 'New order'),
    ('Pending', 'Order is pending processing'),
    ('Processing', 'Order is being processed')
ON CONFLICT (code)
DO UPDATE SET
    description = EXCLUDED.description;
