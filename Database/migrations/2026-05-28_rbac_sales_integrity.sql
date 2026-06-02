-- Day 1 - RBAC + sales detail + ticket integrity

CREATE TABLE IF NOT EXISTS PERMISSIONS (
    id_permission INT AUTO_INCREMENT PRIMARY KEY,
    code VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    active BOOLEAN DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS EMPLOYEE_ROLES (
    id_employee_role INT AUTO_INCREMENT PRIMARY KEY,
    id_staff INT NOT NULL,
    id_rol_staff INT NOT NULL,
    active BOOLEAN DEFAULT TRUE,
    CONSTRAINT EMPLOYEE_ROLES_ibfk_1 FOREIGN KEY (id_staff) REFERENCES STAFF(id_staff),
    CONSTRAINT EMPLOYEE_ROLES_ibfk_2 FOREIGN KEY (id_rol_staff) REFERENCES ROL_STAFF(id_rol_staff),
    UNIQUE KEY uq_employee_role (id_staff, id_rol_staff)
);

CREATE TABLE IF NOT EXISTS ROLE_PERMISSIONS (
    id_role_permission INT AUTO_INCREMENT PRIMARY KEY,
    id_rol_staff INT NOT NULL,
    id_permission INT NOT NULL,
    active BOOLEAN DEFAULT TRUE,
    CONSTRAINT ROLE_PERMISSIONS_ibfk_1 FOREIGN KEY (id_rol_staff) REFERENCES ROL_STAFF(id_rol_staff),
    CONSTRAINT ROLE_PERMISSIONS_ibfk_2 FOREIGN KEY (id_permission) REFERENCES PERMISSIONS(id_permission),
    UNIQUE KEY uq_role_permission (id_rol_staff, id_permission)
);

CREATE TABLE IF NOT EXISTS SALE_DETAILS (
    id_sale_detail INT AUTO_INCREMENT PRIMARY KEY,
    id_venta INT NOT NULL,
    id_evento_asiento INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    quantity INT NOT NULL DEFAULT 1,
    subtotal DECIMAL(10,2) NOT NULL,
    CONSTRAINT SALE_DETAILS_ibfk_1 FOREIGN KEY (id_venta) REFERENCES VENTAS(id_venta),
    CONSTRAINT SALE_DETAILS_ibfk_2 FOREIGN KEY (id_evento_asiento) REFERENCES EVENTO_ASIENTO(id_evento_asiento),
    UNIQUE KEY uq_sale_detail (id_venta, id_evento_asiento)
);

CREATE UNIQUE INDEX idx_tickets_codigo ON TICKETS(codigo_unico);
CREATE UNIQUE INDEX idx_tickets_qr ON TICKETS(qr_token);
