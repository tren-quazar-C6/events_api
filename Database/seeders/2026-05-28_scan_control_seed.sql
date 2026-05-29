-- Día 1 - Seeders base para estados de ticket y roles de empleados

INSERT IGNORE INTO ESTADO_TICKET (nombre_estado) VALUES
('ACTIVO'),
('USADO'),
('CANCELADO'),
('ANULADO'),
('INACTIVO');

INSERT IGNORE INTO ROL_STAFF (nombre_rol, activo) VALUES
('TAQUILLA', 1),
('ACCESO', 1),
('SUPERVISOR', 1),
('ADMIN', 1);
