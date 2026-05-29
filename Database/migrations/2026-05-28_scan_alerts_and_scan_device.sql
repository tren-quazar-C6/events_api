-- Día 1 - Migración base para control de escaneo y antifraude

ALTER TABLE SCAN
    ADD COLUMN dispositivo VARCHAR(120) NULL AFTER observacion;

CREATE TABLE IF NOT EXISTS SCAN_ALERTS (
    id_scan_alert INT AUTO_INCREMENT PRIMARY KEY,
    id_scan INT NULL,
    id_ticket INT NULL,
    id_staff INT NULL,
    tipo_alerta VARCHAR(60) NOT NULL,
    detalle TEXT NULL,
    qr_token VARCHAR(500) NULL,
    dispositivo VARCHAR(120) NULL,
    payload_json JSON NULL,
    fecha_alerta DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT SCAN_ALERTS_ibfk_1 FOREIGN KEY (id_scan) REFERENCES SCAN(id_scan) ON DELETE SET NULL,
    CONSTRAINT SCAN_ALERTS_ibfk_2 FOREIGN KEY (id_ticket) REFERENCES TICKETS(id_ticket) ON DELETE SET NULL,
    CONSTRAINT SCAN_ALERTS_ibfk_3 FOREIGN KEY (id_staff) REFERENCES STAFF(id_staff) ON DELETE SET NULL
);

CREATE INDEX idx_scan_alert_fecha ON SCAN_ALERTS(fecha_alerta);
CREATE INDEX idx_scan_alert_tipo ON SCAN_ALERTS(tipo_alerta);
