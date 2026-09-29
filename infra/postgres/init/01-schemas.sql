-- Fase 0: un unico cluster Postgres, un esquema logico por microservicio.
-- Cuando haga falta, cada esquema puede migrar a su propio servidor sin
-- cambiar el diseño de los servicios (solo la cadena de conexion).

CREATE SCHEMA IF NOT EXISTS catalog;
CREATE SCHEMA IF NOT EXISTS booking;
CREATE SCHEMA IF NOT EXISTS payments;
CREATE SCHEMA IF NOT EXISTS notifications;
