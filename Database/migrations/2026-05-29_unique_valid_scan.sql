-- Enforce one successful access scan per ticket.
-- MySQL allows multiple NULL values in a UNIQUE index, so duplicate/invalid
-- attempts remain loggable while exactly one VALIDO scan is allowed.
ALTER TABLE SCAN
    ADD COLUMN id_ticket_valido INT
        GENERATED ALWAYS AS (
            CASE
                WHEN resultado = 'VALIDO' THEN id_ticket
                ELSE NULL
            END
        ) STORED;

CREATE UNIQUE INDEX uq_scan_ticket_valido
    ON SCAN(id_ticket_valido);
