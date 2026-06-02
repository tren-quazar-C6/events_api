-- Day 1 - RBAC seed

INSERT IGNORE INTO PERMISSIONS (code, name, active) VALUES
('sales.create', 'Crear ventas', 1),
('tickets.generate', 'Generar tickets', 1),
('events.read', 'Consultar eventos', 1),
('employees.me.read', 'Consultar perfil propio', 1);

-- Assign all permissions to ADMIN
INSERT IGNORE INTO ROLE_PERMISSIONS (id_rol_staff, id_permission, active)
SELECT rs.id_rol_staff, p.id_permission, 1
FROM ROL_STAFF rs
CROSS JOIN PERMISSIONS p
WHERE rs.nombre_rol = 'ADMIN';

-- Basic access role
INSERT IGNORE INTO ROLE_PERMISSIONS (id_rol_staff, id_permission, active)
SELECT rs.id_rol_staff, p.id_permission, 1
FROM ROL_STAFF rs
JOIN PERMISSIONS p ON p.code IN ('events.read', 'employees.me.read')
WHERE rs.nombre_rol IN ('ACCESO', 'TAQUILLA', 'SUPERVISOR');
