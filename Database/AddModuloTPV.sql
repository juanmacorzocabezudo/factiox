DROP PROCEDURE IF EXISTS `FactioX_AplicarModuloTPV`;

DELIMITER //

CREATE PROCEDURE `FactioX_AplicarModuloTPV`()
BEGIN
	IF DATABASE() IS NULL THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Seleccione la base de datos de FactioX antes de ejecutar el script.';
	END IF;

	IF (SELECT COUNT(*) FROM information_schema.TABLES
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME IN ('Empresas', 'ConfiguracionEmpresa', 'LineasDocumento', 'Clientes',
			'FormasPagoPersonalizadas', 'Usuarios', 'Facturas', 'Presupuestos')) <> 8 THEN
		SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Faltan tablas base de FactioX. No se puede aplicar la actualizacion del TPV.';
	END IF;

	-- Configuracion de series y opciones TPV
	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'SerieTicket') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `SerieTicket` VARCHAR(10) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'T';
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'NumeroTicketActual') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `NumeroTicketActual` INT NOT NULL DEFAULT 0;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'LongitudNumeroTicket') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `LongitudNumeroTicket` INT NOT NULL DEFAULT 6;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'IncluirAnioEnSerieTicket') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `IncluirAnioEnSerieTicket` TINYINT(1) NOT NULL DEFAULT 1;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'SerieDevolucionTicket') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `SerieDevolucionTicket` VARCHAR(10) CHARACTER SET utf8mb4 NOT NULL DEFAULT 'TD';
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'NumeroDevolucionTicketActual') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `NumeroDevolucionTicketActual` INT NOT NULL DEFAULT 0;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'LongitudNumeroDevolucionTicket') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `LongitudNumeroDevolucionTicket` INT NOT NULL DEFAULT 6;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'IncluirAnioEnSerieDevolucionTicket') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `IncluirAnioEnSerieDevolucionTicket` TINYINT(1) NOT NULL DEFAULT 1;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'TpvDescontarStock') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `TpvDescontarStock` TINYINT(1) NOT NULL DEFAULT 1;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'TpvPermitirStockNegativo') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `TpvPermitirStockNegativo` TINYINT(1) NOT NULL DEFAULT 0;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'TpvPreciosIncluyenIVA') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `TpvPreciosIncluyenIVA` TINYINT(1) NOT NULL DEFAULT 1;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'ConfiguracionEmpresa' AND COLUMN_NAME = 'TpvTextoPieTicket') THEN
		ALTER TABLE `ConfiguracionEmpresa` ADD COLUMN `TpvTextoPieTicket` VARCHAR(500) CHARACTER SET utf8mb4 NULL;
	END IF;

	-- Modulos por empresa
	CREATE TABLE IF NOT EXISTS `EmpresaModulos` (
		`Id` INT NOT NULL AUTO_INCREMENT,
		`EmpresaId` INT NOT NULL,
		`Modulo` VARCHAR(30) CHARACTER SET utf8mb4 NOT NULL,
		`Activo` TINYINT(1) NOT NULL,
		`FechaActivacion` DATETIME(6) NOT NULL,
		`FechaDesactivacion` DATETIME(6) NULL,
		`UsuarioActivacionId` INT NULL,
		PRIMARY KEY (`Id`),
		UNIQUE INDEX `IX_EmpresaModulos_EmpresaId_Modulo` (`EmpresaId`, `Modulo`),
		CONSTRAINT `FK_EmpresaModulos_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE
	) CHARACTER SET = utf8mb4;

	CREATE TABLE IF NOT EXISTS `TpvTerminales` (
		`Id` INT NOT NULL AUTO_INCREMENT,
		`Nombre` VARCHAR(100) CHARACTER SET utf8mb4 NOT NULL,
		`Codigo` VARCHAR(20) CHARACTER SET utf8mb4 NOT NULL,
		`Activo` TINYINT(1) NOT NULL,
		`SerieTicket` VARCHAR(10) CHARACTER SET utf8mb4 NULL,
		`NumeroTicketActual` INT NOT NULL DEFAULT 0,
		`ClienteGenericoId` INT NULL,
		`FormaPagoPorDefectoId` INT NULL,
		`ImpresoraAnchoMm` INT NOT NULL,
		`EmpresaId` INT NOT NULL,
		PRIMARY KEY (`Id`),
		UNIQUE INDEX `IX_TpvTerminales_EmpresaId_Codigo` (`EmpresaId`, `Codigo`),
		INDEX `IX_TpvTerminales_ClienteGenericoId` (`ClienteGenericoId`),
		INDEX `IX_TpvTerminales_FormaPagoPorDefectoId` (`FormaPagoPorDefectoId`),
		CONSTRAINT `FK_TpvTerminales_Clientes_ClienteGenericoId` FOREIGN KEY (`ClienteGenericoId`) REFERENCES `Clientes` (`Id`) ON DELETE SET NULL,
		CONSTRAINT `FK_TpvTerminales_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE CASCADE,
		CONSTRAINT `FK_TpvTerminales_FormasPagoPersonalizadas_FormaPagoPorDefectoId` FOREIGN KEY (`FormaPagoPorDefectoId`) REFERENCES `FormasPagoPersonalizadas` (`Id`) ON DELETE SET NULL
	) CHARACTER SET = utf8mb4;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TpvTerminales' AND COLUMN_NAME = 'SerieTicket') THEN
		ALTER TABLE `TpvTerminales` ADD COLUMN `SerieTicket` VARCHAR(10) CHARACTER SET utf8mb4 NULL;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'TpvTerminales' AND COLUMN_NAME = 'NumeroTicketActual') THEN
		ALTER TABLE `TpvTerminales` ADD COLUMN `NumeroTicketActual` INT NOT NULL DEFAULT 0;
	END IF;

	CREATE TABLE IF NOT EXISTS `TpvSesionesCaja` (
		`Id` INT NOT NULL AUTO_INCREMENT,
		`TerminalId` INT NOT NULL,
		`UsuarioAperturaId` INT NOT NULL,
		`FechaApertura` DATETIME(6) NOT NULL,
		`FondoInicial` DECIMAL(18,2) NOT NULL,
		`UsuarioCierreId` INT NULL,
		`FechaCierre` DATETIME(6) NULL,
		`EfectivoContado` DECIMAL(18,2) NULL,
		`EfectivoEsperado` DECIMAL(18,2) NOT NULL,
		`Descuadre` DECIMAL(18,2) NOT NULL,
		`Estado` VARCHAR(20) CHARACTER SET utf8mb4 NOT NULL,
		`Observaciones` VARCHAR(1000) CHARACTER SET utf8mb4 NULL,
		`EmpresaId` INT NOT NULL,
		PRIMARY KEY (`Id`),
		INDEX `IX_TpvSesionesCaja_EmpresaId` (`EmpresaId`),
		INDEX `IX_TpvSesionesCaja_TerminalId_Estado` (`TerminalId`, `Estado`),
		CONSTRAINT `FK_TpvSesionesCaja_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TpvSesionesCaja_TpvTerminales_TerminalId` FOREIGN KEY (`TerminalId`) REFERENCES `TpvTerminales` (`Id`) ON DELETE RESTRICT
	) CHARACTER SET = utf8mb4;

	CREATE TABLE IF NOT EXISTS `TicketsTpv` (
		`Id` INT NOT NULL AUTO_INCREMENT,
		`Numero` VARCHAR(50) CHARACTER SET utf8mb4 NULL,
		`Fecha` DATETIME(6) NOT NULL,
		`FechaCobro` DATETIME(6) NULL,
		`TerminalId` INT NOT NULL,
		`SesionCajaId` INT NULL,
		`UsuarioId` INT NOT NULL,
		`ClienteId` INT NULL,
		`Estado` VARCHAR(20) CHARACTER SET utf8mb4 NOT NULL,
		`TipoTicket` VARCHAR(20) CHARACTER SET utf8mb4 NOT NULL,
		`TicketOriginalId` INT NULL,
		`PresupuestoOrigenId` INT NULL,
		`FacturaId` INT NULL,
		`DescuentoGlobal` DECIMAL(5,2) NOT NULL,
		`BaseImponible` DECIMAL(18,2) NOT NULL,
		`TotalIVA` DECIMAL(18,2) NOT NULL,
		`Total` DECIMAL(18,2) NOT NULL,
		`Entregado` DECIMAL(18,2) NOT NULL,
		`Cambio` DECIMAL(18,2) NOT NULL,
		`Notas` VARCHAR(500) CHARACTER SET utf8mb4 NULL,
		`Huella` VARCHAR(64) CHARACTER SET utf8mb4 NULL,
		`EmpresaId` INT NOT NULL,
		PRIMARY KEY (`Id`),
		UNIQUE INDEX `IX_TicketsTpv_EmpresaId_Numero` (`EmpresaId`, `Numero`),
		INDEX `IX_TicketsTpv_EmpresaId_Estado` (`EmpresaId`, `Estado`),
		INDEX `IX_TicketsTpv_EmpresaId_Fecha` (`EmpresaId`, `Fecha`),
		INDEX `IX_TicketsTpv_ClienteId` (`ClienteId`),
		INDEX `IX_TicketsTpv_FacturaId` (`FacturaId`),
		INDEX `IX_TicketsTpv_PresupuestoOrigenId` (`PresupuestoOrigenId`),
		INDEX `IX_TicketsTpv_SesionCajaId` (`SesionCajaId`),
		INDEX `IX_TicketsTpv_TerminalId` (`TerminalId`),
		INDEX `IX_TicketsTpv_TicketOriginalId` (`TicketOriginalId`),
		INDEX `IX_TicketsTpv_UsuarioId` (`UsuarioId`),
		CONSTRAINT `FK_TicketsTpv_Clientes_ClienteId` FOREIGN KEY (`ClienteId`) REFERENCES `Clientes` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TicketsTpv_Empresas_EmpresaId` FOREIGN KEY (`EmpresaId`) REFERENCES `Empresas` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TicketsTpv_Facturas_FacturaId` FOREIGN KEY (`FacturaId`) REFERENCES `Facturas` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TicketsTpv_Presupuestos_PresupuestoOrigenId` FOREIGN KEY (`PresupuestoOrigenId`) REFERENCES `Presupuestos` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TicketsTpv_TicketsTpv_TicketOriginalId` FOREIGN KEY (`TicketOriginalId`) REFERENCES `TicketsTpv` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TicketsTpv_TpvSesionesCaja_SesionCajaId` FOREIGN KEY (`SesionCajaId`) REFERENCES `TpvSesionesCaja` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TicketsTpv_TpvTerminales_TerminalId` FOREIGN KEY (`TerminalId`) REFERENCES `TpvTerminales` (`Id`) ON DELETE RESTRICT,
		CONSTRAINT `FK_TicketsTpv_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `Usuarios` (`Id`) ON DELETE RESTRICT
	) CHARACTER SET = utf8mb4;

	CREATE TABLE IF NOT EXISTS `TpvMovimientosCaja` (
		`Id` INT NOT NULL AUTO_INCREMENT,
		`SesionCajaId` INT NOT NULL,
		`Tipo` VARCHAR(20) CHARACTER SET utf8mb4 NOT NULL,
		`Importe` DECIMAL(18,2) NOT NULL,
		`Concepto` VARCHAR(200) CHARACTER SET utf8mb4 NOT NULL,
		`UsuarioId` INT NOT NULL,
		`Fecha` DATETIME(6) NOT NULL,
		PRIMARY KEY (`Id`),
		INDEX `IX_TpvMovimientosCaja_SesionCajaId` (`SesionCajaId`),
		CONSTRAINT `FK_TpvMovimientosCaja_TpvSesionesCaja_SesionCajaId` FOREIGN KEY (`SesionCajaId`) REFERENCES `TpvSesionesCaja` (`Id`) ON DELETE CASCADE
	) CHARACTER SET = utf8mb4;

	CREATE TABLE IF NOT EXISTS `PagosTicketTpv` (
		`Id` INT NOT NULL AUTO_INCREMENT,
		`TicketTpvId` INT NOT NULL,
		`FormaPagoPersonalizadaId` INT NULL,
		`EsEfectivo` TINYINT(1) NOT NULL,
		`Importe` DECIMAL(18,2) NOT NULL,
		`Referencia` VARCHAR(100) CHARACTER SET utf8mb4 NULL,
		PRIMARY KEY (`Id`),
		INDEX `IX_PagosTicketTpv_FormaPagoPersonalizadaId` (`FormaPagoPersonalizadaId`),
		INDEX `IX_PagosTicketTpv_TicketTpvId` (`TicketTpvId`),
		CONSTRAINT `FK_PagosTicketTpv_FormasPagoPersonalizadas_FormaPagoPersonaliza~` FOREIGN KEY (`FormaPagoPersonalizadaId`) REFERENCES `FormasPagoPersonalizadas` (`Id`) ON DELETE SET NULL,
		CONSTRAINT `FK_PagosTicketTpv_TicketsTpv_TicketTpvId` FOREIGN KEY (`TicketTpvId`) REFERENCES `TicketsTpv` (`Id`) ON DELETE CASCADE
	) CHARACTER SET = utf8mb4;

	-- Lineas de ticket (reutilizan LineasDocumento)
	IF NOT EXISTS (SELECT 1 FROM information_schema.COLUMNS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LineasDocumento' AND COLUMN_NAME = 'TicketTpvId') THEN
		ALTER TABLE `LineasDocumento` ADD COLUMN `TicketTpvId` INT NULL;
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.STATISTICS
		WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'LineasDocumento' AND INDEX_NAME = 'IX_LineasDocumento_TicketTpvId') THEN
		CREATE INDEX `IX_LineasDocumento_TicketTpvId` ON `LineasDocumento` (`TicketTpvId`);
	END IF;

	IF NOT EXISTS (SELECT 1 FROM information_schema.REFERENTIAL_CONSTRAINTS
		WHERE CONSTRAINT_SCHEMA = DATABASE() AND TABLE_NAME = 'LineasDocumento' AND CONSTRAINT_NAME = 'FK_LineasDocumento_TicketsTpv_TicketTpvId') THEN
		ALTER TABLE `LineasDocumento` ADD CONSTRAINT `FK_LineasDocumento_TicketsTpv_TicketTpvId`
			FOREIGN KEY (`TicketTpvId`) REFERENCES `TicketsTpv` (`Id`) ON DELETE CASCADE;
	END IF;

	-- Terminal por defecto para las empresas que ya tengan el TPV activo
	INSERT INTO `TpvTerminales` (`Nombre`, `Codigo`, `Activo`, `ClienteGenericoId`, `FormaPagoPorDefectoId`, `ImpresoraAnchoMm`, `EmpresaId`)
	SELECT 'Caja 1', 'CAJA1', 1, NULL, NULL, 80, m.`EmpresaId`
	FROM `EmpresaModulos` m
	WHERE m.`Modulo` = 'TPV' AND m.`Activo` = 1
		AND NOT EXISTS (SELECT 1 FROM `TpvTerminales` t WHERE t.`EmpresaId` = m.`EmpresaId`);

	CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
		`MigrationId` VARCHAR(150) CHARACTER SET utf8mb4 NOT NULL,
		`ProductVersion` VARCHAR(32) CHARACTER SET utf8mb4 NOT NULL,
		PRIMARY KEY (`MigrationId`)
	);

	INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
	SELECT '20261013150423_AddModuloTPV', '8.0.11'
	WHERE NOT EXISTS (SELECT 1 FROM `__EFMigrationsHistory`
		WHERE `MigrationId` = '20261013150423_AddModuloTPV');
END//

DELIMITER ;

CALL `FactioX_AplicarModuloTPV`();
DROP PROCEDURE `FactioX_AplicarModuloTPV`;

SELECT 'Actualizacion del modulo TPV completada' AS Resultado, DATABASE() AS BaseDeDatos;

SELECT `EmpresaId`, `SerieTicket`, `NumeroTicketActual`, `LongitudNumeroTicket`, `IncluirAnioEnSerieTicket`,
	   `SerieDevolucionTicket`, `NumeroDevolucionTicketActual`, `TpvDescontarStock`, `TpvPermitirStockNegativo`
FROM `ConfiguracionEmpresa`
ORDER BY `EmpresaId`;

-- Para activar el TPV en una empresa sin pasar por la pantalla de Empresas:
-- INSERT INTO `EmpresaModulos` (`EmpresaId`, `Modulo`, `Activo`, `FechaActivacion`) VALUES (<EmpresaId>, 'TPV', 1, NOW(6))
--   ON DUPLICATE KEY UPDATE `Activo` = 1, `FechaActivacion` = NOW(6), `FechaDesactivacion` = NULL;
