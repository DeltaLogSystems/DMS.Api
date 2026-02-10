CREATE DATABASE  IF NOT EXISTS `dms_db` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `dms_db`;
-- MySQL dump 10.13  Distrib 8.0.44, for Win64 (x86_64)
--
-- Host: 212.38.94.213    Database: dms_db
-- ------------------------------------------------------
-- Server version	8.0.45-0ubuntu0.24.04.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `l_center_configuration`
--

DROP TABLE IF EXISTS `l_center_configuration`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `l_center_configuration` (
  `ConfigurationID` int NOT NULL AUTO_INCREMENT,
  `CenterID` int NOT NULL,
  `MachineSessionHours` decimal(10,2) DEFAULT NULL,
  `IsFixedHoursForSession` bit(1) DEFAULT b'0',
  `CenterOpenTime` time DEFAULT '08:00:00',
  `CenterCloseTime` time DEFAULT '20:00:00',
  `SlotDuration` int DEFAULT '240' COMMENT 'Duration in minutes (default 4 hours)',
  PRIMARY KEY (`ConfigurationID`),
  KEY `CenterID` (`CenterID`),
  CONSTRAINT `l_center_configuration_ibfk_1` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `l_center_configuration`
--

LOCK TABLES `l_center_configuration` WRITE;
/*!40000 ALTER TABLE `l_center_configuration` DISABLE KEYS */;
INSERT INTO `l_center_configuration` VALUES (1,1,4.00,_binary '','08:00:00','24:00:00',60);
/*!40000 ALTER TABLE `l_center_configuration` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_appointmentstatus`
--

DROP TABLE IF EXISTS `m_appointmentstatus`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_appointmentstatus` (
  `StatusID` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(50) NOT NULL,
  `StatusColor` varchar(20) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` date NOT NULL,
  `CreatedBy` varchar(50) NOT NULL,
  PRIMARY KEY (`StatusID`),
  UNIQUE KEY `StatusName` (`StatusName`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_appointmentstatus`
--

LOCK TABLES `m_appointmentstatus` WRITE;
/*!40000 ALTER TABLE `m_appointmentstatus` DISABLE KEYS */;
INSERT INTO `m_appointmentstatus` VALUES (1,'Scheduled','#4CAF50',_binary '','2026-01-14','system'),(2,'Confirmed','#2196F3',_binary '','2026-01-14','system'),(3,'In Progress','#FF9800',_binary '','2026-01-14','system'),(4,'Completed','#009688',_binary '','2026-01-14','system'),(5,'Cancelled','#F44336',_binary '','2026-01-14','system'),(6,'No Show','#9E9E9E',_binary '','2026-01-14','system'),(7,'Rescheduled','#FFC107',_binary '','2026-01-14','system');
/*!40000 ALTER TABLE `m_appointmentstatus` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_asset_types`
--

DROP TABLE IF EXISTS `m_asset_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_asset_types` (
  `AssetTypeID` int NOT NULL AUTO_INCREMENT,
  `AssetTypeName` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `AssetTypeCode` varchar(10) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `RequiresMaintenance` bit(1) DEFAULT b'0',
  `MaintenanceIntervalDays` int DEFAULT '30',
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  PRIMARY KEY (`AssetTypeID`),
  UNIQUE KEY `AssetTypeName` (`AssetTypeName`),
  UNIQUE KEY `AssetTypeCode` (`AssetTypeCode`),
  KEY `idx_active` (`IsActive`)
) ENGINE=InnoDB AUTO_INCREMENT=7 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_asset_types`
--

LOCK TABLES `m_asset_types` WRITE;
/*!40000 ALTER TABLE `m_asset_types` DISABLE KEYS */;
INSERT INTO `m_asset_types` VALUES (1,'Dialysis Machine','DM','Hemodialysis machines for patient treatment',_binary '',30,_binary '','2026-01-17 19:59:45',1,NULL,NULL),(2,'Computer','COMP','Desktop and laptop computers',_binary '\0',0,_binary '','2026-01-17 19:59:45',1,'2026-01-17 21:58:57',1),(3,'Furniture','FURN','Chairs, beds, and other furniture',_binary '\0',0,_binary '','2026-01-17 19:59:45',1,NULL,NULL),(4,'Medical Equipment','MED','Blood pressure monitors, weighing scales, etc.',_binary '',90,_binary '','2026-01-17 19:59:45',1,NULL,NULL),(5,'Infrastructure','INFRA','Water treatment plant, generators, etc.',_binary '',180,_binary '','2026-01-17 19:59:45',1,NULL,NULL),(6,'Test Asset','TEST','sdadadasdsadasdasd',_binary '\0',0,_binary '','2026-01-17 21:58:34',1,'2026-01-17 21:58:41',1);
/*!40000 ALTER TABLE `m_asset_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_assets`
--

DROP TABLE IF EXISTS `m_assets`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_assets` (
  `AssetID` int NOT NULL AUTO_INCREMENT,
  `AssetCode` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `AssetName` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `AssetType` int NOT NULL,
  `SerialNo` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ModelNo` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Manufacturer` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `PurchaseDate` date DEFAULT NULL,
  `PurchaseCost` decimal(10,2) DEFAULT NULL,
  `WarrantyExpiryDate` date DEFAULT NULL,
  `CenterID` int NOT NULL,
  `CompanyID` int NOT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `InactiveReason` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `InactiveDate` datetime DEFAULT NULL,
  `ExpectedActiveDate` date DEFAULT NULL,
  `LastMaintenanceDate` date DEFAULT NULL,
  `NextMaintenanceDate` date DEFAULT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  PRIMARY KEY (`AssetID`),
  UNIQUE KEY `AssetCode` (`AssetCode`),
  KEY `CompanyID` (`CompanyID`),
  KEY `idx_center_active` (`CenterID`,`IsActive`),
  KEY `idx_asset_type` (`AssetType`),
  KEY `idx_serial` (`SerialNo`),
  CONSTRAINT `m_assets_ibfk_1` FOREIGN KEY (`AssetType`) REFERENCES `m_asset_types` (`AssetTypeID`),
  CONSTRAINT `m_assets_ibfk_2` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`),
  CONSTRAINT `m_assets_ibfk_3` FOREIGN KEY (`CompanyID`) REFERENCES `m_companies` (`CompanyID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_assets`
--

LOCK TABLES `m_assets` WRITE;
/*!40000 ALTER TABLE `m_assets` DISABLE KEYS */;
INSERT INTO `m_assets` VALUES (1,'DM-NEP-0001','Dialysis Machine',1,'DIA-001','DIA-001','Mangesh Dialysis Machine Manufacturers','2024-01-22',1000000.00,'2036-01-31',1,1,_binary '','Asset reactivated and ready for use',NULL,NULL,NULL,'2024-02-21','2026-01-18 10:07:05',1,'2026-01-24 11:45:21',1),(2,'DM-NEP-0002','Dialysis Machine',1,'DIA-002','DIA-002','Mangesh Dialysis Machine Manufacturers',NULL,1000000.00,NULL,1,1,_binary '',NULL,NULL,NULL,NULL,'2026-02-23','2026-01-24 19:43:26',1,NULL,NULL);
/*!40000 ALTER TABLE `m_assets` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_centers`
--

DROP TABLE IF EXISTS `m_centers`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_centers` (
  `CenterID` int NOT NULL AUTO_INCREMENT,
  `CompanyID` int NOT NULL,
  `CenterName` varchar(100) NOT NULL,
  `CenterAddress` varchar(100) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` date NOT NULL,
  `CreatedBy` varchar(50) NOT NULL,
  PRIMARY KEY (`CenterID`),
  KEY `CompanyID` (`CompanyID`),
  CONSTRAINT `m_centers_ibfk_1` FOREIGN KEY (`CompanyID`) REFERENCES `m_companies` (`CompanyID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_centers`
--

LOCK TABLES `m_centers` WRITE;
/*!40000 ALTER TABLE `m_centers` DISABLE KEYS */;
INSERT INTO `m_centers` VALUES (1,1,'Nephro Dialysis Centre','Navle Bridge, Pune',_binary '','2026-01-03','system');
/*!40000 ALTER TABLE `m_centers` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_companies`
--

DROP TABLE IF EXISTS `m_companies`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_companies` (
  `CompanyID` int NOT NULL AUTO_INCREMENT,
  `CompanyName` varchar(100) NOT NULL,
  `CompanyCode` varchar(10) NOT NULL,
  `CompanyAddress` varchar(255) DEFAULT NULL,
  `CompanyLogo` varchar(100) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` date NOT NULL,
  `CreatedBy` varchar(50) NOT NULL,
  PRIMARY KEY (`CompanyID`),
  UNIQUE KEY `CompanyCode` (`CompanyCode`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_companies`
--

LOCK TABLES `m_companies` WRITE;
/*!40000 ALTER TABLE `m_companies` DISABLE KEYS */;
INSERT INTO `m_companies` VALUES (1,'Multicare Health Services','MHS','Pune','src/assets/images/company_logos/multicare.png',_binary '','2026-01-03','system');
/*!40000 ALTER TABLE `m_companies` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_inventory_item_types`
--

DROP TABLE IF EXISTS `m_inventory_item_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_inventory_item_types` (
  `ItemTypeID` int NOT NULL AUTO_INCREMENT,
  `ItemTypeName` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ItemTypeCode` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  PRIMARY KEY (`ItemTypeID`),
  UNIQUE KEY `ItemTypeName` (`ItemTypeName`),
  UNIQUE KEY `ItemTypeCode` (`ItemTypeCode`),
  KEY `idx_active` (`IsActive`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_inventory_item_types`
--

LOCK TABLES `m_inventory_item_types` WRITE;
/*!40000 ALTER TABLE `m_inventory_item_types` DISABLE KEYS */;
INSERT INTO `m_inventory_item_types` VALUES (1,'DIALYZER F6','DLSR','Dialyser filters for hemodialysis',_binary '','2026-01-18 10:23:18',1,'2026-01-24 11:09:56',1),(2,'Blood Tubing Set','BTS','Blood lines and tubing',_binary '','2026-01-18 10:23:18',1,NULL,NULL),(3,'AV Fistula Needles','AVN','Arteriovenous fistula needles',_binary '','2026-01-18 10:23:18',1,NULL,NULL),(5,'Heparin','HEP','Anticoagulant medication',_binary '','2026-01-18 10:23:18',1,NULL,NULL),(6,'Gloves','GLV','Disposable medical gloves',_binary '','2026-01-18 10:23:18',1,NULL,NULL),(7,'Disinfectant','DIS','Surface and equipment disinfectant',_binary '','2026-01-18 10:23:18',1,NULL,NULL),(8,'Bicarbonate Cartridge','BIC','Dialysate bicarbonate',_binary '','2026-01-18 10:23:18',1,NULL,NULL),(9,'Injections ','INJ',NULL,_binary '','2026-01-23 21:04:08',1,NULL,NULL),(10,'ON/OFF KIT','SUG','Initiating and ending hemodialysis',_binary '','2026-01-24 09:54:45',1,'2026-01-24 09:59:15',1),(11,'NS 1000ml /500ml','IV','Priming the Dialysis circuit',_binary '','2026-01-24 10:07:34',1,'2026-01-24 10:09:34',1),(17,'BECTODINE OINTMENT ','OIN ','AV Fistula and Catheter exit side care',_binary '','2026-01-24 10:14:09',1,NULL,NULL),(19,'IV SET','IS','Normal saline and medication infusion',_binary '','2026-01-24 10:16:27',1,NULL,NULL),(20,'SYRINGE 10cc','DISPO','Heparin/Saline medication administration',_binary '','2026-01-24 10:18:16',1,NULL,NULL),(21,'SYRINGE 5cc','NIPRO','Catheter care/blood samples',_binary '','2026-01-24 10:19:36',1,NULL,NULL),(22,'RENACLIN','RNC','Disinfecting Dialyzer and Tubing',_binary '','2026-01-24 10:22:01',1,NULL,NULL),(23,'CITRO','CTL','Disinfection of dialysis machines and equipment',_binary '','2026-01-24 10:23:35',1,NULL,NULL),(24,'FIXMOULL STRETCH 10*10','BSN','Catheter Dressing',_binary '','2026-01-24 10:25:34',1,NULL,NULL),(25,'GAUZE 200GM','SAI','AV Fistula / Catheter care',_binary '','2026-01-24 10:27:19',1,NULL,NULL),(26,'COTTON 400GM','CTN','Sterile swabs',_binary '','2026-01-24 10:29:06',1,NULL,NULL),(27,'TRANSDUCER PROTECTAR','TP','Prevent blood entry in to dialysis machine',_binary '','2026-01-24 10:31:53',1,NULL,NULL),(28,'GLUCO STREEP','IHL','Check blood sugar level',_binary '','2026-01-24 10:33:34',1,NULL,NULL),(29,'FACE MASK','ACC','Protection for the bacteria',_binary '','2026-01-24 10:35:25',1,NULL,NULL),(30,'SANITIZER ','SNT','Hands hygiene',_binary '','2026-01-24 10:37:28',1,NULL,NULL),(31,'PAPER TEP','PPT','Securing dialysis needle/catheter dressing',_binary '','2026-01-24 10:40:06',1,NULL,NULL),(32,'DI-SAFE FILTER ','DI','Improving dialysate quality',_binary '','2026-01-24 10:41:43',1,NULL,NULL),(33,'DEXTROZE D25','DEX','Treatment of hypoglycaemia',_binary '','2026-01-24 10:43:17',1,NULL,NULL),(34,'SPIRIT ','SRT','Prevention infection',_binary '','2026-01-24 10:45:12',1,NULL,NULL),(35,'CHIPLADEEN SOLUTION ','SOL','Skin antiseptic',_binary '','2026-01-24 10:47:12',1,NULL,NULL),(36,'POLYMED DIALYZER ','PMD','Dialyser filters for hemodialysis',_binary '','2026-01-24 11:10:34',1,NULL,NULL);
/*!40000 ALTER TABLE `m_inventory_item_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_inventory_items`
--

DROP TABLE IF EXISTS `m_inventory_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_inventory_items` (
  `InventoryItemID` int NOT NULL AUTO_INCREMENT,
  `ItemCode` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ItemName` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `ItemTypeID` int NOT NULL,
  `UsageTypeID` int NOT NULL,
  `Description` text COLLATE utf8mb4_unicode_ci,
  `Manufacturer` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `MinimumUsageCount` int DEFAULT '1',
  `MaximumUsageCount` int DEFAULT '1',
  `IsIndividualQtyTracking` bit(1) DEFAULT b'0',
  `RequiresApprovalForEarlyDiscard` bit(1) DEFAULT b'0',
  `RequiresApprovalForOveruse` bit(1) DEFAULT b'0',
  `IsRequiredForDialysis` bit(1) DEFAULT b'0',
  `UnitOfMeasure` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ReorderLevel` int DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  PRIMARY KEY (`InventoryItemID`),
  UNIQUE KEY `ItemCode` (`ItemCode`),
  KEY `idx_item_type` (`ItemTypeID`),
  KEY `idx_usage_type` (`UsageTypeID`),
  KEY `idx_required_dialysis` (`IsRequiredForDialysis`),
  CONSTRAINT `m_inventory_items_ibfk_1` FOREIGN KEY (`ItemTypeID`) REFERENCES `m_inventory_item_types` (`ItemTypeID`),
  CONSTRAINT `m_inventory_items_ibfk_2` FOREIGN KEY (`UsageTypeID`) REFERENCES `m_inventory_usage_types` (`UsageTypeID`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_inventory_items`
--

LOCK TABLES `m_inventory_items` WRITE;
/*!40000 ALTER TABLE `m_inventory_items` DISABLE KEYS */;
INSERT INTO `m_inventory_items` VALUES (1,'DLSR-0001','Dialyser',1,2,'xcvcxvxcv','Mangesh Dialysis Machine Manufacturers',5,10,_binary '',_binary '',_binary '\0',_binary '','box',NULL,_binary '','2026-01-18 12:53:18',1,'2026-01-18 12:55:39',1),(2,'AVN-0001','Needles',3,1,'cvbvb','Mangesh Dialysis Machine Manufacturers',1,1,_binary '\0',_binary '\0',_binary '\0',_binary '','box',NULL,_binary '','2026-01-21 22:53:51',1,NULL,NULL),(3,'INJ-0001','Emset',9,1,'Inj.Emset','Dora',1,1,_binary '\0',_binary '\0',_binary '\0',_binary '\0','Mg',NULL,_binary '','2026-01-23 21:05:15',1,NULL,NULL);
/*!40000 ALTER TABLE `m_inventory_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_inventory_usage_types`
--

DROP TABLE IF EXISTS `m_inventory_usage_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_inventory_usage_types` (
  `UsageTypeID` int NOT NULL AUTO_INCREMENT,
  `UsageTypeName` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `UsageTypeCode` varchar(10) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  PRIMARY KEY (`UsageTypeID`),
  UNIQUE KEY `UsageTypeName` (`UsageTypeName`),
  UNIQUE KEY `UsageTypeCode` (`UsageTypeCode`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_inventory_usage_types`
--

LOCK TABLES `m_inventory_usage_types` WRITE;
/*!40000 ALTER TABLE `m_inventory_usage_types` DISABLE KEYS */;
INSERT INTO `m_inventory_usage_types` VALUES (1,'Single Use','SU','Dispose after one use',_binary ''),(2,'Multi Use','MU','Can be reused multiple times',_binary ''),(3,'Consumable','CONS','Quantity-based consumable items',_binary '');
/*!40000 ALTER TABLE `m_inventory_usage_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_patients`
--

DROP TABLE IF EXISTS `m_patients`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_patients` (
  `PatientID` int NOT NULL AUTO_INCREMENT,
  `PatientCode` varchar(20) NOT NULL,
  `PatientName` varchar(100) NOT NULL,
  `CompanyID` int NOT NULL,
  `CenterID` int NOT NULL,
  `MobileNo` varchar(15) DEFAULT NULL,
  `DateOfBirth` date NOT NULL,
  `Age` int NOT NULL,
  `SchemeType` int DEFAULT NULL,
  `DialysisCycles` int DEFAULT '0',
  `CurrentCycleNumber` int DEFAULT '1',
  `CurrentCycleStartDate` date DEFAULT NULL,
  `CurrentCycleEndDate` date DEFAULT NULL,
  `CurrentCycleSessionCount` int DEFAULT '0',
  `TotalCompletedCycles` int DEFAULT '0',
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedBy` int NOT NULL,
  `CreatedDate` date NOT NULL,
  `ModifiedBy` int DEFAULT NULL,
  `ModifiedDate` date DEFAULT NULL,
  PRIMARY KEY (`PatientID`),
  UNIQUE KEY `PatientCode` (`PatientCode`),
  UNIQUE KEY `MobileNo` (`MobileNo`),
  KEY `CenterID` (`CenterID`),
  KEY `idx_patient_code` (`PatientCode`),
  KEY `idx_mobile` (`MobileNo`),
  KEY `idx_company_center` (`CompanyID`,`CenterID`),
  KEY `fk_patient_scheme` (`SchemeType`),
  CONSTRAINT `fk_patient_scheme` FOREIGN KEY (`SchemeType`) REFERENCES `m_schemetypes` (`SchemeTypeID`),
  CONSTRAINT `m_patients_ibfk_1` FOREIGN KEY (`CompanyID`) REFERENCES `m_companies` (`CompanyID`),
  CONSTRAINT `m_patients_ibfk_2` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`)
) ENGINE=InnoDB AUTO_INCREMENT=11 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_patients`
--

LOCK TABLES `m_patients` WRITE;
/*!40000 ALTER TABLE `m_patients` DISABLE KEYS */;
INSERT INTO `m_patients` VALUES (1,'MHS0001','Shubham',1,1,'7888113878','2000-02-21',25,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-03',1,'2026-01-17'),(2,'MHS0002','Dipak Pawar',1,1,'9657575279','1997-07-05',28,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-15',1,'2026-01-18'),(3,'MHS0003','Tasalim ansari ',1,1,'9011203569','2002-01-08',24,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-19',NULL,NULL),(4,'MHS0004','Amar Khalate',1,1,'7888113879','1999-02-12',26,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-21',NULL,NULL),(5,'MHS0005','Umesh ravindra Gavade',1,1,'9921364967','2000-06-01',25,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-22',NULL,NULL),(6,'MHS0006','Rushi Ardad',1,1,'9090090958','2001-09-16',24,4,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-23',NULL,NULL),(7,'MHS0007','Bhumika ',1,1,'8208017660','2006-06-12',19,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-23',NULL,NULL),(8,'MHS0008','Mangesh ',1,1,'9156358318','1999-01-27',26,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-23',NULL,NULL),(9,'MHS0009','Sushma',1,1,'8208225547','1994-01-07',32,NULL,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-23',NULL,NULL),(10,'MHS0010','Upendra chargundi',1,1,'8999694718','2001-01-19',25,1,0,1,NULL,NULL,0,0,_binary '',1,'2026-01-24',1,'2026-02-03');
/*!40000 ALTER TABLE `m_patients` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_roles`
--

DROP TABLE IF EXISTS `m_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_roles` (
  `RoleID` int NOT NULL AUTO_INCREMENT,
  `RoleName` varchar(50) NOT NULL,
  PRIMARY KEY (`RoleID`),
  UNIQUE KEY `RoleName` (`RoleName`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_roles`
--

LOCK TABLES `m_roles` WRITE;
/*!40000 ALTER TABLE `m_roles` DISABLE KEYS */;
INSERT INTO `m_roles` VALUES (1,'Administrator');
/*!40000 ALTER TABLE `m_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_schemetypes`
--

DROP TABLE IF EXISTS `m_schemetypes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_schemetypes` (
  `SchemeTypeID` int NOT NULL AUTO_INCREMENT,
  `SchemeTypeName` varchar(50) NOT NULL,
  `Description` varchar(255) DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` date NOT NULL,
  `CreatedBy` varchar(50) NOT NULL,
  PRIMARY KEY (`SchemeTypeID`),
  UNIQUE KEY `SchemeTypeName` (`SchemeTypeName`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_schemetypes`
--

LOCK TABLES `m_schemetypes` WRITE;
/*!40000 ALTER TABLE `m_schemetypes` DISABLE KEYS */;
INSERT INTO `m_schemetypes` VALUES (1,'MJPJAY','Mahatma Jyotiba Phule Jan Arogya Yojana',_binary '','2026-01-03','system'),(2,'PMJAY','Pradhan Mantri Jan Arogya Yojana',_binary '','2026-01-03','system'),(3,'PMC','Pune Municipal Corporation',_binary '','2026-01-03','system'),(4,'Self Pay','Self Payment',_binary '','2026-01-03','system'),(5,'Others','Other Schemes',_binary '','2026-01-03','system');
/*!40000 ALTER TABLE `m_schemetypes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_session_note_types`
--

DROP TABLE IF EXISTS `m_session_note_types`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_session_note_types` (
  `NoteTypeID` int NOT NULL AUTO_INCREMENT,
  `NoteTypeName` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `NoteTypeCode` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UnitOfMeasure` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `IsMandatory` bit(1) DEFAULT b'0',
  `IsNumeric` bit(1) DEFAULT b'1',
  `MinimumValue` decimal(10,2) DEFAULT NULL,
  `MaximumValue` decimal(10,2) DEFAULT NULL,
  `DefaultValue` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DisplayOrder` int DEFAULT '0',
  `Category` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  PRIMARY KEY (`NoteTypeID`),
  UNIQUE KEY `NoteTypeName` (`NoteTypeName`),
  UNIQUE KEY `NoteTypeCode` (`NoteTypeCode`),
  KEY `idx_active` (`IsActive`),
  KEY `idx_category` (`Category`),
  KEY `idx_mandatory` (`IsMandatory`)
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_session_note_types`
--

LOCK TABLES `m_session_note_types` WRITE;
/*!40000 ALTER TABLE `m_session_note_types` DISABLE KEYS */;
INSERT INTO `m_session_note_types` VALUES (1,'Blood Pressure (Systolic)','BP_SYS',NULL,'mmHg',_binary '',_binary '',70.00,250.00,NULL,1,'VitalSigns',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(2,'Blood Pressure (Diastolic)','BP_DIA','','mmHg',_binary '\0',_binary '',40.00,150.00,'',2,'VitalSigns',_binary '','2026-01-18 19:42:20',1,'2026-02-09 20:58:08',0),(3,'Pulse Rate','PULSE','','bpm',_binary '\0',_binary '',40.00,200.00,'',3,'VitalSigns',_binary '','2026-01-18 19:42:20',1,'2026-02-09 20:58:03',0),(4,'SpO2 (Oxygen Saturation)','SPO2','','%',_binary '\0',_binary '',70.00,100.00,'',4,'VitalSigns',_binary '','2026-01-18 19:42:20',1,'2026-02-09 20:58:00',0),(5,'Temperature','TEMP',NULL,'°F',_binary '\0',_binary '',95.00,106.00,NULL,5,'VitalSigns',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(6,'Blood Sugar','SUGAR','','mg/dL',_binary '\0',_binary '',50.00,500.00,'',6,'LabResults',_binary '','2026-01-18 19:42:20',1,'2026-02-09 20:57:53',0),(7,'Weight (Pre-Dialysis)','WT_PRE','','kg',_binary '\0',_binary '',20.00,300.00,'',7,'LabResults',_binary '','2026-01-18 19:42:20',1,'2026-02-09 20:57:48',0),(8,'Weight (Post-Dialysis)','WT_POST','','kg',_binary '\0',_binary '',20.00,300.00,'',8,'LabResults',_binary '','2026-01-18 19:42:20',1,'2026-02-09 20:57:42',0),(9,'Blood Flow Rate','BFR',NULL,'mL/min',_binary '\0',_binary '',200.00,500.00,NULL,9,'Treatment',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(10,'Dialysate Flow Rate','DFR',NULL,'mL/min',_binary '\0',_binary '',500.00,800.00,NULL,10,'Treatment',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(11,'Ultrafiltration Goal','UF_GOAL',NULL,'L',_binary '\0',_binary '',0.00,5.00,NULL,11,'Treatment',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(12,'Patient Condition','CONDITION',NULL,NULL,_binary '\0',_binary '\0',NULL,NULL,NULL,12,'Observations',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(13,'Complications','COMPLICATIONS',NULL,NULL,_binary '\0',_binary '\0',NULL,NULL,NULL,13,'Observations',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(14,'Other Notes','OTHER',NULL,NULL,_binary '\0',_binary '\0',NULL,NULL,NULL,14,'Other',_binary '','2026-01-18 19:42:20',1,NULL,NULL),(15,'Testv','BP','f','',_binary '\0',_binary '',56.00,66.00,'',15,'VitalSigns',_binary '','2026-01-20 22:16:28',1,'2026-02-09 20:57:36',0),(16,'Venous Pressure','VP','','mmHg',_binary '\0',_binary '',50.00,200.00,'',16,'Observations',_binary '','2026-01-24 12:16:13',1,NULL,NULL),(17,'Conductivity','C','','mS/cm',_binary '\0',_binary '',13.50,14.50,'',17,'Other',_binary '','2026-01-24 12:19:13',1,NULL,NULL);
/*!40000 ALTER TABLE `m_session_note_types` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_session_status`
--

DROP TABLE IF EXISTS `m_session_status`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_session_status` (
  `StatusID` int NOT NULL AUTO_INCREMENT,
  `StatusName` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `StatusCode` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `DisplayOrder` int DEFAULT '0',
  `IsActive` bit(1) DEFAULT b'1',
  PRIMARY KEY (`StatusID`),
  UNIQUE KEY `StatusName` (`StatusName`),
  UNIQUE KEY `StatusCode` (`StatusCode`)
) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_session_status`
--

LOCK TABLES `m_session_status` WRITE;
/*!40000 ALTER TABLE `m_session_status` DISABLE KEYS */;
INSERT INTO `m_session_status` VALUES (1,'Not Started','NOT_STARTED','Session not yet started',1,_binary ''),(2,'In Progress','IN_PROGRESS','Session is currently running',2,_binary ''),(3,'On Hold','ON_HOLD','Session temporarily paused',3,_binary ''),(4,'Completed','COMPLETED','Session completed successfully',4,_binary ''),(5,'Terminated','TERMINATED','Session terminated due to complications',5,_binary '');
/*!40000 ALTER TABLE `m_session_status` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `m_users`
--

DROP TABLE IF EXISTS `m_users`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `m_users` (
  `UserID` int NOT NULL AUTO_INCREMENT,
  `FirstName` varchar(50) NOT NULL,
  `LastName` varchar(50) NOT NULL,
  `EmailID` varchar(50) NOT NULL,
  `MobileNo` varchar(10) DEFAULT NULL,
  `UserName` varchar(10) NOT NULL,
  `CompanyID` int NOT NULL,
  `CenterID` int NOT NULL,
  `UserRole` int NOT NULL,
  `IsSuperUser` bit(1) DEFAULT b'0',
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` date NOT NULL,
  `CreatedBy` varchar(50) NOT NULL,
  `ModifiedBy` varchar(50) DEFAULT NULL,
  `ModifiedDate` date DEFAULT NULL,
  `Password` varchar(255) NOT NULL DEFAULT 'temp123',
  PRIMARY KEY (`UserID`),
  UNIQUE KEY `EmailID` (`EmailID`),
  UNIQUE KEY `UserName` (`UserName`),
  KEY `CompanyID` (`CompanyID`),
  KEY `CenterID` (`CenterID`),
  KEY `UserRole` (`UserRole`),
  CONSTRAINT `m_users_ibfk_1` FOREIGN KEY (`CompanyID`) REFERENCES `m_companies` (`CompanyID`),
  CONSTRAINT `m_users_ibfk_2` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`),
  CONSTRAINT `m_users_ibfk_3` FOREIGN KEY (`UserRole`) REFERENCES `m_roles` (`RoleID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `m_users`
--

LOCK TABLES `m_users` WRITE;
/*!40000 ALTER TABLE `m_users` DISABLE KEYS */;
INSERT INTO `m_users` VALUES (1,'Mangesh','Nivangune','mangesh15121999@gmail.com','8888412712','MN001',1,1,1,_binary '\0',_binary '','2026-01-03','system','system','2026-01-03','mn123');
/*!40000 ALTER TABLE `m_users` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_appointments`
--

DROP TABLE IF EXISTS `t_appointments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_appointments` (
  `AppointmentID` int NOT NULL AUTO_INCREMENT,
  `PatientID` int NOT NULL,
  `CenterID` int NOT NULL,
  `CompanyID` int NOT NULL,
  `AppointmentStatus` int NOT NULL DEFAULT '1',
  `AppointmentDate` date NOT NULL,
  `CreatedBy` int NOT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsRescheduled` bit(1) DEFAULT b'0',
  `RescheduleRevision` int DEFAULT '0',
  `RescheduleReason` varchar(255) DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  PRIMARY KEY (`AppointmentID`),
  KEY `CompanyID` (`CompanyID`),
  KEY `AppointmentStatus` (`AppointmentStatus`),
  KEY `idx_appointment_date` (`AppointmentDate`),
  KEY `idx_patient_date` (`PatientID`,`AppointmentDate`),
  KEY `idx_center_date` (`CenterID`,`AppointmentDate`),
  CONSTRAINT `t_appointments_ibfk_1` FOREIGN KEY (`PatientID`) REFERENCES `m_patients` (`PatientID`),
  CONSTRAINT `t_appointments_ibfk_2` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`),
  CONSTRAINT `t_appointments_ibfk_3` FOREIGN KEY (`CompanyID`) REFERENCES `m_companies` (`CompanyID`),
  CONSTRAINT `t_appointments_ibfk_4` FOREIGN KEY (`AppointmentStatus`) REFERENCES `m_appointmentstatus` (`StatusID`)
) ENGINE=InnoDB AUTO_INCREMENT=32 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_appointments`
--

LOCK TABLES `t_appointments` WRITE;
/*!40000 ALTER TABLE `t_appointments` DISABLE KEYS */;
INSERT INTO `t_appointments` VALUES (1,1,1,1,1,'2026-01-16',1,'2026-01-15 22:08:51',_binary '\0',0,NULL,NULL,NULL),(2,1,1,1,1,'2026-01-17',1,'2026-01-16 22:00:08',_binary '\0',0,NULL,NULL,NULL),(3,2,1,1,5,'2026-01-18',1,'2026-01-16 22:15:34',_binary '\0',0,NULL,1,'2026-01-17 10:52:01'),(4,2,1,1,5,'2026-01-18',1,'2026-01-17 10:52:49',_binary '\0',0,NULL,1,'2026-01-17 11:29:45'),(5,2,1,1,5,'2026-01-18',1,'2026-01-17 11:30:06',_binary '\0',0,NULL,1,'2026-01-17 11:31:39'),(6,1,1,1,1,'2026-01-20',1,'2026-01-17 13:44:05',_binary '\0',0,NULL,NULL,NULL),(7,1,1,1,1,'2026-01-19',1,'2026-01-19 10:11:24',_binary '\0',0,NULL,NULL,NULL),(8,2,1,1,1,'2026-01-20',1,'2026-01-19 22:44:03',_binary '\0',0,NULL,NULL,NULL),(9,3,1,1,2,'2026-01-20',1,'2026-01-20 22:19:25',_binary '\0',0,NULL,1,'2026-01-20 22:38:22'),(10,4,1,1,2,'2026-01-21',1,'2026-01-21 13:38:14',_binary '\0',0,NULL,0,'2026-01-21 22:51:01'),(11,5,1,1,3,'2026-01-23',1,'2026-01-22 21:05:13',_binary '\0',0,NULL,0,'2026-01-23 22:53:42'),(12,7,1,1,2,'2026-01-24',1,'2026-01-23 18:30:48',_binary '\0',0,NULL,0,'2026-01-23 18:32:54'),(13,6,1,1,2,'2026-01-24',1,'2026-01-23 18:30:54',_binary '\0',0,NULL,0,'2026-01-23 18:33:45'),(14,8,1,1,5,'2026-01-24',1,'2026-01-23 18:30:59',_binary '',2,'Rescheduled by user',1,'2026-01-23 18:32:26'),(15,8,1,1,7,'2026-01-23',1,'2026-01-23 18:33:04',_binary '',3,'Rescheduled by user',1,'2026-01-23 18:34:35'),(16,1,1,1,2,'2026-01-24',1,'2026-01-23 20:49:39',_binary '\0',0,NULL,0,'2026-01-24 20:30:27'),(17,10,1,1,2,'2026-01-24',1,'2026-01-24 11:44:36',_binary '\0',0,NULL,0,'2026-01-24 11:46:50'),(18,1,1,1,2,'2026-01-25',1,'2026-01-24 19:44:18',_binary '\0',0,NULL,1,'2026-01-25 10:32:51'),(19,2,1,1,1,'2026-01-25',1,'2026-01-24 20:23:21',_binary '\0',0,NULL,NULL,NULL),(20,1,1,1,2,'2026-01-27',1,'2026-01-27 16:35:54',_binary '\0',0,NULL,1,'2026-01-27 16:37:55'),(21,2,1,1,1,'2026-01-27',1,'2026-01-27 16:36:38',_binary '\0',0,NULL,NULL,NULL),(22,4,1,1,2,'2026-02-04',1,'2026-02-04 13:56:32',_binary '\0',0,NULL,1,'2026-02-04 16:16:30'),(23,1,1,1,1,'2026-02-04',1,'2026-02-04 21:48:17',_binary '\0',0,NULL,NULL,NULL),(24,1,1,1,2,'2026-02-05',1,'2026-02-05 08:35:14',_binary '\0',0,NULL,1,'2026-02-05 08:35:33'),(25,1,1,1,3,'2026-02-07',1,'2026-02-07 19:51:38',_binary '\0',0,NULL,0,'2026-02-07 23:47:24'),(26,2,1,1,2,'2026-02-07',1,'2026-02-07 19:54:04',_binary '\0',0,NULL,1,'2026-02-07 19:54:17'),(27,1,1,1,3,'2026-02-09',1,'2026-02-09 07:39:53',_binary '\0',0,NULL,0,'2026-02-09 20:58:45'),(28,2,1,1,3,'2026-02-09',1,'2026-02-09 07:40:12',_binary '\0',0,NULL,0,'2026-02-09 20:58:30'),(29,4,1,1,3,'2026-02-09',1,'2026-02-09 21:24:12',_binary '\0',0,NULL,0,'2026-02-09 21:25:10'),(30,8,1,1,2,'2026-02-09',1,'2026-02-09 22:02:04',_binary '\0',0,NULL,1,'2026-02-09 22:02:24'),(31,1,1,1,2,'2026-02-10',1,'2026-02-10 08:37:05',_binary '\0',0,NULL,1,'2026-02-10 08:37:35');
/*!40000 ALTER TABLE `t_appointments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_asset_assignments`
--

DROP TABLE IF EXISTS `t_asset_assignments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_asset_assignments` (
  `AssignmentID` int NOT NULL AUTO_INCREMENT,
  `AssetID` int NOT NULL,
  `AppointmentID` int NOT NULL,
  `AssignedDate` date NOT NULL,
  `AssignedTime` time NOT NULL,
  `SessionDuration` int DEFAULT NULL,
  `Status` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT 'Active',
  `Notes` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`AssignmentID`),
  KEY `idx_asset_date` (`AssetID`,`AssignedDate`),
  KEY `idx_appointment` (`AppointmentID`),
  CONSTRAINT `t_asset_assignments_ibfk_1` FOREIGN KEY (`AssetID`) REFERENCES `m_assets` (`AssetID`),
  CONSTRAINT `t_asset_assignments_ibfk_2` FOREIGN KEY (`AppointmentID`) REFERENCES `t_appointments` (`AppointmentID`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=18 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_asset_assignments`
--

LOCK TABLES `t_asset_assignments` WRITE;
/*!40000 ALTER TABLE `t_asset_assignments` DISABLE KEYS */;
INSERT INTO `t_asset_assignments` VALUES (1,1,10,'2026-01-21','22:55:00',0,'Active','Active','2026-01-21 22:51:31',0),(2,1,11,'2026-01-22','21:08:00',0,'Completed','Active','2026-01-22 21:08:33',0),(3,1,15,'2026-01-23','18:35:00',0,'Active','Active','2026-01-23 18:35:11',0),(4,1,12,'2026-01-23','18:35:00',0,'Active','Active','2026-01-23 18:35:39',0),(5,1,13,'2026-01-23','18:36:00',0,'Active','Active','2026-01-23 18:36:27',0),(6,1,17,'2026-01-24','11:48:00',0,'Active','Active','2026-01-24 11:48:14',0),(7,1,16,'2026-01-24','20:55:00',0,'Active','Active','2026-01-24 20:55:49',0),(8,1,18,'2026-01-25','10:32:00',1,'Active','Active','2026-01-25 10:32:52',1),(9,1,20,'2026-01-27','16:37:00',1,'Active','Active','2026-01-27 16:37:59',1),(10,1,22,'2026-02-04','16:16:00',1,'Active','Active','2026-02-04 16:16:30',1),(11,1,24,'2026-02-05','08:35:00',1,'Active','Active','2026-02-05 08:35:33',1),(12,1,25,'2026-02-07','19:51:00',1,'Completed','Active','2026-02-07 19:51:50',1),(13,1,27,'2026-02-09','07:45:00',1,'Completed','Active','2026-02-09 07:40:49',1),(14,2,28,'2026-02-09','07:45:00',1,'Completed','Active','2026-02-09 07:41:33',1),(15,1,29,'2026-02-09','21:24:00',1,'Completed','Active','2026-02-09 21:24:22',1),(16,1,30,'2026-02-09','22:02:00',1,'Active','Active','2026-02-09 22:02:25',1),(17,1,31,'2026-02-10','08:37:00',1,'Active','Active','2026-02-10 08:37:35',1);
/*!40000 ALTER TABLE `t_asset_assignments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_asset_maintenance`
--

DROP TABLE IF EXISTS `t_asset_maintenance`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_asset_maintenance` (
  `MaintenanceID` int NOT NULL AUTO_INCREMENT,
  `AssetID` int NOT NULL,
  `MaintenanceDate` date NOT NULL,
  `MaintenanceType` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Description` text COLLATE utf8mb4_unicode_ci,
  `TechnicianName` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Cost` decimal(10,2) DEFAULT NULL,
  `NextMaintenanceDate` date DEFAULT NULL,
  `Status` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT 'Completed',
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`MaintenanceID`),
  KEY `idx_asset_date` (`AssetID`,`MaintenanceDate`),
  CONSTRAINT `t_asset_maintenance_ibfk_1` FOREIGN KEY (`AssetID`) REFERENCES `m_assets` (`AssetID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_asset_maintenance`
--

LOCK TABLES `t_asset_maintenance` WRITE;
/*!40000 ALTER TABLE `t_asset_maintenance` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_asset_maintenance` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_asset_status_history`
--

DROP TABLE IF EXISTS `t_asset_status_history`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_asset_status_history` (
  `HistoryID` int NOT NULL AUTO_INCREMENT,
  `AssetID` int NOT NULL,
  `PreviousStatus` bit(1) NOT NULL,
  `NewStatus` bit(1) NOT NULL,
  `Reason` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ExpectedActiveDate` date DEFAULT NULL,
  `ChangedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ChangedBy` int NOT NULL,
  PRIMARY KEY (`HistoryID`),
  KEY `idx_asset_history` (`AssetID`,`ChangedDate`),
  CONSTRAINT `t_asset_status_history_ibfk_1` FOREIGN KEY (`AssetID`) REFERENCES `m_assets` (`AssetID`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_asset_status_history`
--

LOCK TABLES `t_asset_status_history` WRITE;
/*!40000 ALTER TABLE `t_asset_status_history` DISABLE KEYS */;
INSERT INTO `t_asset_status_history` VALUES (1,1,_binary '',_binary '\0','Monitor is not working','2026-01-30','2026-01-18 10:07:53',1),(2,1,_binary '\0',_binary '','Asset reactivated and ready for use',NULL,'2026-01-18 13:00:38',1),(3,1,_binary '',_binary '\0','Maintenance ','2026-01-24','2026-01-23 20:56:55',1),(4,1,_binary '\0',_binary '','Asset reactivated and ready for use',NULL,'2026-01-24 11:45:21',1);
/*!40000 ALTER TABLE `t_asset_status_history` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_dialysis_sessions`
--

DROP TABLE IF EXISTS `t_dialysis_sessions`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_dialysis_sessions` (
  `SessionID` int NOT NULL AUTO_INCREMENT,
  `SessionCode` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `AppointmentID` int NOT NULL,
  `PatientID` int NOT NULL,
  `CenterID` int NOT NULL,
  `AssetID` int DEFAULT NULL,
  `AssetAssignmentID` int DEFAULT NULL,
  `SessionStatus` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT 'Not Started',
  `SessionDate` date NOT NULL,
  `ScheduledStartTime` time DEFAULT NULL,
  `ActualStartTime` datetime DEFAULT NULL,
  `ActualEndTime` datetime DEFAULT NULL,
  `SessionDuration` int DEFAULT NULL,
  `DialysisType` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `StartedBy` int DEFAULT NULL,
  `CompletedBy` int DEFAULT NULL,
  `PreSessionNotes` text COLLATE utf8mb4_unicode_ci,
  `PostSessionNotes` text COLLATE utf8mb4_unicode_ci,
  `TerminationReason` text COLLATE utf8mb4_unicode_ci,
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  PRIMARY KEY (`SessionID`),
  UNIQUE KEY `SessionCode` (`SessionCode`),
  KEY `AssetID` (`AssetID`),
  KEY `AssetAssignmentID` (`AssetAssignmentID`),
  KEY `idx_appointment` (`AppointmentID`),
  KEY `idx_patient` (`PatientID`),
  KEY `idx_session_date` (`SessionDate`),
  KEY `idx_status` (`SessionStatus`),
  KEY `idx_center` (`CenterID`),
  CONSTRAINT `t_dialysis_sessions_ibfk_1` FOREIGN KEY (`AppointmentID`) REFERENCES `t_appointments` (`AppointmentID`),
  CONSTRAINT `t_dialysis_sessions_ibfk_2` FOREIGN KEY (`PatientID`) REFERENCES `m_patients` (`PatientID`),
  CONSTRAINT `t_dialysis_sessions_ibfk_3` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`),
  CONSTRAINT `t_dialysis_sessions_ibfk_4` FOREIGN KEY (`AssetID`) REFERENCES `m_assets` (`AssetID`),
  CONSTRAINT `t_dialysis_sessions_ibfk_5` FOREIGN KEY (`AssetAssignmentID`) REFERENCES `t_asset_assignments` (`AssignmentID`)
) ENGINE=InnoDB AUTO_INCREMENT=28 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_dialysis_sessions`
--

LOCK TABLES `t_dialysis_sessions` WRITE;
/*!40000 ALTER TABLE `t_dialysis_sessions` DISABLE KEYS */;
INSERT INTO `t_dialysis_sessions` VALUES (10,'SES-NDC-20260121-001',10,4,1,1,1,'Not Started','2026-01-21','14:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,'fsdfsd',NULL,NULL,'2026-01-21 22:51:00',0,'2026-01-21 22:51:31',0),(11,'SES-NDC-20260123-001',11,5,1,1,2,'Completed','2026-01-23','10:00:00','2026-01-23 22:52:54','2026-01-23 22:53:42',0,'Hemodialysis',0,0,NULL,'Session completed successfully',NULL,'2026-01-22 21:07:32',0,'2026-01-23 22:53:42',0),(12,'SES-NDC-20260124-001',12,7,1,1,4,'Not Started','2026-01-24','08:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-01-23 18:32:54',0,'2026-01-23 18:35:39',0),(13,'SES-NDC-20260124-002',15,8,1,1,3,'Not Started','2026-01-24','09:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,'Ckd',NULL,NULL,'2026-01-23 18:33:37',0,'2026-01-23 18:35:11',0),(14,'SES-NDC-20260124-003',13,6,1,1,5,'Not Started','2026-01-24','11:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,'Breathless ',NULL,NULL,'2026-01-23 18:33:45',0,'2026-01-23 18:36:27',0),(15,'SES-NDC-20260124-004',17,10,1,1,6,'Not Started','2026-01-24','12:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,'Clear ',NULL,NULL,'2026-01-24 11:46:50',0,'2026-01-24 11:48:14',0),(16,'SES-NDC-20260124-005',16,1,1,1,7,'Not Started','2026-01-24','09:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,'sfsdf',NULL,NULL,'2026-01-24 20:30:27',0,'2026-01-24 20:55:49',0),(17,'SES-NDC-20260125-001',18,1,1,1,8,'Not Started','2026-01-25','08:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-01-25 10:32:51',1,'2026-01-25 10:32:52',1),(18,'SES-NDC-20260127-001',20,1,1,1,9,'Not Started','2026-01-27','17:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-01-27 16:37:55',1,'2026-01-27 16:37:59',1),(19,'SES-NDC-20260204-001',22,4,1,1,10,'Not Started','2026-02-04','14:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-02-04 16:16:30',1,'2026-02-04 16:16:30',1),(20,'SES-NDC-20260205-001',24,1,1,1,11,'Not Started','2026-02-05','09:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-02-05 08:35:33',1,'2026-02-05 08:35:33',1),(21,'SES-NDC-20260207-001',25,1,1,1,12,'Completed','2026-02-07','20:00:00','2026-02-07 19:52:10','2026-02-07 23:47:24',235,'Hemodialysis',1,0,NULL,'Session completed successfully',NULL,'2026-02-07 19:51:49',1,'2026-02-07 23:47:24',0),(22,'SES-NDC-20260207-002',26,2,1,NULL,NULL,'Not Started','2026-02-07','20:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-02-07 19:54:17',1,NULL,NULL),(23,'SES-NDC-20260209-001',27,1,1,1,13,'Completed','2026-02-09','08:00:00','2026-02-09 07:41:11','2026-02-09 20:58:45',797,'Hemodialysis',1,0,NULL,'Session completed successfully',NULL,'2026-02-09 07:40:49',1,'2026-02-09 20:58:45',0),(24,'SES-NDC-20260209-002',28,2,1,2,14,'Completed','2026-02-09','08:00:00','2026-02-09 07:41:41','2026-02-09 20:58:30',796,'Hemodialysis',1,0,NULL,'Session completed successfully',NULL,'2026-02-09 07:41:33',1,'2026-02-09 20:58:30',0),(25,'SES-NDC-20260209-003',29,4,1,1,15,'Completed','2026-02-09','22:00:00','2026-02-09 21:24:33','2026-02-09 21:25:10',0,'Hemodialysis',1,0,NULL,'Session completed successfully',NULL,'2026-02-09 21:24:22',1,'2026-02-09 21:25:10',0),(26,'SES-NDC-20260209-004',30,8,1,1,16,'Not Started','2026-02-09','23:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-02-09 22:02:24',1,'2026-02-09 22:02:25',1),(27,'SES-NDC-20260210-001',31,1,1,1,17,'Not Started','2026-02-10','09:00:00',NULL,NULL,NULL,'Hemodialysis',NULL,NULL,NULL,NULL,NULL,'2026-02-10 08:37:35',1,'2026-02-10 08:37:35',1);
/*!40000 ALTER TABLE `t_dialysis_sessions` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_inventory_adjustments`
--

DROP TABLE IF EXISTS `t_inventory_adjustments`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_inventory_adjustments` (
  `AdjustmentID` int NOT NULL AUTO_INCREMENT,
  `StockID` int NOT NULL,
  `CenterID` int NOT NULL,
  `AdjustmentType` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `QuantityAdjusted` int NOT NULL,
  `Reason` text COLLATE utf8mb4_unicode_ci,
  `ReferenceNumber` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `AdjustmentDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `AdjustedBy` int NOT NULL,
  `ApprovedBy` int DEFAULT NULL,
  PRIMARY KEY (`AdjustmentID`),
  KEY `CenterID` (`CenterID`),
  KEY `idx_stock` (`StockID`),
  KEY `idx_date` (`AdjustmentDate`),
  CONSTRAINT `t_inventory_adjustments_ibfk_1` FOREIGN KEY (`StockID`) REFERENCES `t_inventory_stock` (`StockID`),
  CONSTRAINT `t_inventory_adjustments_ibfk_2` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_inventory_adjustments`
--

LOCK TABLES `t_inventory_adjustments` WRITE;
/*!40000 ALTER TABLE `t_inventory_adjustments` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_inventory_adjustments` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_inventory_discard_requests`
--

DROP TABLE IF EXISTS `t_inventory_discard_requests`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_inventory_discard_requests` (
  `RequestID` int NOT NULL AUTO_INCREMENT,
  `IndividualItemID` int NOT NULL,
  `InventoryItemID` int NOT NULL,
  `CenterID` int NOT NULL,
  `RequestType` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `CurrentUsageCount` int NOT NULL,
  `MinimumUsageCount` int NOT NULL,
  `Reason` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `RequestStatus` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT 'Pending',
  `RequestedBy` int NOT NULL,
  `RequestedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ReviewedBy` int DEFAULT NULL,
  `ReviewedDate` datetime DEFAULT NULL,
  `ReviewComments` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`RequestID`),
  KEY `IndividualItemID` (`IndividualItemID`),
  KEY `InventoryItemID` (`InventoryItemID`),
  KEY `idx_status` (`RequestStatus`),
  KEY `idx_center` (`CenterID`),
  CONSTRAINT `t_inventory_discard_requests_ibfk_1` FOREIGN KEY (`IndividualItemID`) REFERENCES `t_inventory_individual_items` (`IndividualItemID`),
  CONSTRAINT `t_inventory_discard_requests_ibfk_2` FOREIGN KEY (`InventoryItemID`) REFERENCES `m_inventory_items` (`InventoryItemID`),
  CONSTRAINT `t_inventory_discard_requests_ibfk_3` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_inventory_discard_requests`
--

LOCK TABLES `t_inventory_discard_requests` WRITE;
/*!40000 ALTER TABLE `t_inventory_discard_requests` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_inventory_discard_requests` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_inventory_individual_items`
--

DROP TABLE IF EXISTS `t_inventory_individual_items`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_inventory_individual_items` (
  `IndividualItemID` int NOT NULL AUTO_INCREMENT,
  `StockID` int NOT NULL,
  `InventoryItemID` int NOT NULL,
  `CenterID` int NOT NULL,
  `IndividualItemCode` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `SerialNumber` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CurrentUsageCount` int DEFAULT '0',
  `MaxUsageCount` int NOT NULL,
  `ItemStatus` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT 'Available',
  `IsAvailable` bit(1) DEFAULT b'1',
  `FirstUsedDate` datetime DEFAULT NULL,
  `LastUsedDate` datetime DEFAULT NULL,
  `DiscardedDate` datetime DEFAULT NULL,
  `DiscardReason` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  PRIMARY KEY (`IndividualItemID`),
  UNIQUE KEY `IndividualItemCode` (`IndividualItemCode`),
  KEY `InventoryItemID` (`InventoryItemID`),
  KEY `CenterID` (`CenterID`),
  KEY `idx_stock` (`StockID`),
  KEY `idx_status` (`ItemStatus`),
  KEY `idx_available` (`IsAvailable`),
  CONSTRAINT `t_inventory_individual_items_ibfk_1` FOREIGN KEY (`StockID`) REFERENCES `t_inventory_stock` (`StockID`) ON DELETE CASCADE,
  CONSTRAINT `t_inventory_individual_items_ibfk_2` FOREIGN KEY (`InventoryItemID`) REFERENCES `m_inventory_items` (`InventoryItemID`),
  CONSTRAINT `t_inventory_individual_items_ibfk_3` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`)
) ENGINE=InnoDB AUTO_INCREMENT=131 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_inventory_individual_items`
--

LOCK TABLES `t_inventory_individual_items` WRITE;
/*!40000 ALTER TABLE `t_inventory_individual_items` DISABLE KEYS */;
INSERT INTO `t_inventory_individual_items` VALUES (1,12,1,1,'DLSR-0001-001',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:50',1),(2,12,1,1,'DLSR-0001-002',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:51',1),(3,12,1,1,'DLSR-0001-003',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:51',1),(4,12,1,1,'DLSR-0001-004',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:51',1),(5,12,1,1,'DLSR-0001-005',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:51',1),(6,12,1,1,'DLSR-0001-006',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:51',1),(7,12,1,1,'DLSR-0001-007',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:51',1),(8,12,1,1,'DLSR-0001-008',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:52',1),(9,12,1,1,'DLSR-0001-009',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:52',1),(10,12,1,1,'DLSR-0001-010',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-18 15:11:52',1),(11,14,1,1,'DLSR-0001-011',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(12,14,1,1,'DLSR-0001-012',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(13,14,1,1,'DLSR-0001-013',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(14,14,1,1,'DLSR-0001-014',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(15,14,1,1,'DLSR-0001-015',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(16,14,1,1,'DLSR-0001-016',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(17,14,1,1,'DLSR-0001-017',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(18,14,1,1,'DLSR-0001-018',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(19,14,1,1,'DLSR-0001-019',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(20,14,1,1,'DLSR-0001-020',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(21,14,1,1,'DLSR-0001-021',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(22,14,1,1,'DLSR-0001-022',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(23,14,1,1,'DLSR-0001-023',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(24,14,1,1,'DLSR-0001-024',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(25,14,1,1,'DLSR-0001-025',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(26,14,1,1,'DLSR-0001-026',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(27,14,1,1,'DLSR-0001-027',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(28,14,1,1,'DLSR-0001-028',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(29,14,1,1,'DLSR-0001-029',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(30,14,1,1,'DLSR-0001-030',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-01-23 21:03:13',1),(31,16,1,1,'DLSR-0001-031',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(32,16,1,1,'DLSR-0001-032',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(33,16,1,1,'DLSR-0001-033',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(34,16,1,1,'DLSR-0001-034',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(35,16,1,1,'DLSR-0001-035',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(36,16,1,1,'DLSR-0001-036',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(37,16,1,1,'DLSR-0001-037',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(38,16,1,1,'DLSR-0001-038',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(39,16,1,1,'DLSR-0001-039',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(40,16,1,1,'DLSR-0001-040',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(41,16,1,1,'DLSR-0001-041',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(42,16,1,1,'DLSR-0001-042',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(43,16,1,1,'DLSR-0001-043',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(44,16,1,1,'DLSR-0001-044',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(45,16,1,1,'DLSR-0001-045',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(46,16,1,1,'DLSR-0001-046',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(47,16,1,1,'DLSR-0001-047',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(48,16,1,1,'DLSR-0001-048',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(49,16,1,1,'DLSR-0001-049',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(50,16,1,1,'DLSR-0001-050',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(51,16,1,1,'DLSR-0001-051',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(52,16,1,1,'DLSR-0001-052',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(53,16,1,1,'DLSR-0001-053',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(54,16,1,1,'DLSR-0001-054',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(55,16,1,1,'DLSR-0001-055',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(56,16,1,1,'DLSR-0001-056',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(57,16,1,1,'DLSR-0001-057',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(58,16,1,1,'DLSR-0001-058',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(59,16,1,1,'DLSR-0001-059',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(60,16,1,1,'DLSR-0001-060',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(61,16,1,1,'DLSR-0001-061',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(62,16,1,1,'DLSR-0001-062',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(63,16,1,1,'DLSR-0001-063',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(64,16,1,1,'DLSR-0001-064',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(65,16,1,1,'DLSR-0001-065',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(66,16,1,1,'DLSR-0001-066',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(67,16,1,1,'DLSR-0001-067',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(68,16,1,1,'DLSR-0001-068',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(69,16,1,1,'DLSR-0001-069',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(70,16,1,1,'DLSR-0001-070',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(71,16,1,1,'DLSR-0001-071',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(72,16,1,1,'DLSR-0001-072',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(73,16,1,1,'DLSR-0001-073',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(74,16,1,1,'DLSR-0001-074',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(75,16,1,1,'DLSR-0001-075',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(76,16,1,1,'DLSR-0001-076',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(77,16,1,1,'DLSR-0001-077',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(78,16,1,1,'DLSR-0001-078',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(79,16,1,1,'DLSR-0001-079',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(80,16,1,1,'DLSR-0001-080',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(81,16,1,1,'DLSR-0001-081',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(82,16,1,1,'DLSR-0001-082',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(83,16,1,1,'DLSR-0001-083',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(84,16,1,1,'DLSR-0001-084',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(85,16,1,1,'DLSR-0001-085',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(86,16,1,1,'DLSR-0001-086',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(87,16,1,1,'DLSR-0001-087',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(88,16,1,1,'DLSR-0001-088',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(89,16,1,1,'DLSR-0001-089',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(90,16,1,1,'DLSR-0001-090',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(91,16,1,1,'DLSR-0001-091',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(92,16,1,1,'DLSR-0001-092',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(93,16,1,1,'DLSR-0001-093',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(94,16,1,1,'DLSR-0001-094',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(95,16,1,1,'DLSR-0001-095',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(96,16,1,1,'DLSR-0001-096',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(97,16,1,1,'DLSR-0001-097',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(98,16,1,1,'DLSR-0001-098',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(99,16,1,1,'DLSR-0001-099',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(100,16,1,1,'DLSR-0001-100',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(101,16,1,1,'DLSR-0001-101',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(102,16,1,1,'DLSR-0001-102',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(103,16,1,1,'DLSR-0001-103',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(104,16,1,1,'DLSR-0001-104',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(105,16,1,1,'DLSR-0001-105',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(106,16,1,1,'DLSR-0001-106',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(107,16,1,1,'DLSR-0001-107',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(108,16,1,1,'DLSR-0001-108',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(109,16,1,1,'DLSR-0001-109',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(110,16,1,1,'DLSR-0001-110',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(111,16,1,1,'DLSR-0001-111',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(112,16,1,1,'DLSR-0001-112',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(113,16,1,1,'DLSR-0001-113',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(114,16,1,1,'DLSR-0001-114',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(115,16,1,1,'DLSR-0001-115',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(116,16,1,1,'DLSR-0001-116',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(117,16,1,1,'DLSR-0001-117',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(118,16,1,1,'DLSR-0001-118',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(119,16,1,1,'DLSR-0001-119',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(120,16,1,1,'DLSR-0001-120',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(121,16,1,1,'DLSR-0001-121',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(122,16,1,1,'DLSR-0001-122',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(123,16,1,1,'DLSR-0001-123',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(124,16,1,1,'DLSR-0001-124',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(125,16,1,1,'DLSR-0001-125',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(126,16,1,1,'DLSR-0001-126',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(127,16,1,1,'DLSR-0001-127',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(128,16,1,1,'DLSR-0001-128',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(129,16,1,1,'DLSR-0001-129',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1),(130,16,1,1,'DLSR-0001-130',NULL,0,10,'Available',_binary '',NULL,NULL,NULL,NULL,'2026-02-09 21:27:24',1);
/*!40000 ALTER TABLE `t_inventory_individual_items` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_inventory_stock`
--

DROP TABLE IF EXISTS `t_inventory_stock`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_inventory_stock` (
  `StockID` int NOT NULL AUTO_INCREMENT,
  `InventoryItemID` int NOT NULL,
  `CenterID` int NOT NULL,
  `CompanyID` int NOT NULL,
  `BatchNumber` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `ManufactureDate` date DEFAULT NULL,
  `ExpiryDate` date DEFAULT NULL,
  `PurchaseDate` date DEFAULT NULL,
  `PurchaseCost` decimal(10,2) DEFAULT NULL,
  `Quantity` int NOT NULL DEFAULT '0',
  `AvailableQuantity` int NOT NULL DEFAULT '0',
  `IsActive` bit(1) DEFAULT b'1',
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CreatedBy` int NOT NULL,
  `ModifiedDate` datetime DEFAULT NULL,
  `ModifiedBy` int DEFAULT NULL,
  PRIMARY KEY (`StockID`),
  KEY `InventoryItemID` (`InventoryItemID`),
  KEY `CompanyID` (`CompanyID`),
  KEY `idx_center_item` (`CenterID`,`InventoryItemID`),
  KEY `idx_expiry` (`ExpiryDate`),
  CONSTRAINT `t_inventory_stock_ibfk_1` FOREIGN KEY (`InventoryItemID`) REFERENCES `m_inventory_items` (`InventoryItemID`),
  CONSTRAINT `t_inventory_stock_ibfk_2` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`),
  CONSTRAINT `t_inventory_stock_ibfk_3` FOREIGN KEY (`CompanyID`) REFERENCES `m_companies` (`CompanyID`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_inventory_stock`
--

LOCK TABLES `t_inventory_stock` WRITE;
/*!40000 ALTER TABLE `t_inventory_stock` DISABLE KEYS */;
INSERT INTO `t_inventory_stock` VALUES (12,1,1,1,'BATCH-2026-01','2026-01-11','2026-01-23','2026-01-16',8000.00,10,10,_binary '','2026-01-18 15:11:50',1,NULL,NULL),(13,2,1,1,NULL,NULL,NULL,NULL,NULL,100,99,_binary '','2026-01-21 22:54:05',1,NULL,NULL),(14,1,1,1,'Batch','2026-01-22','2026-01-30','2026-01-05',5666.00,20,20,_binary '','2026-01-23 21:03:13',1,NULL,NULL),(15,3,1,1,'F2ED3”120','2026-01-22','2027-01-26','2026-01-22',NULL,10,10,_binary '','2026-01-24 11:12:23',1,NULL,NULL),(16,1,1,1,NULL,'2026-01-31','2036-02-28','2026-02-07',800000.00,100,100,_binary '','2026-02-09 21:27:24',1,NULL,NULL);
/*!40000 ALTER TABLE `t_inventory_stock` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_inventory_usage`
--

DROP TABLE IF EXISTS `t_inventory_usage`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_inventory_usage` (
  `UsageID` int NOT NULL AUTO_INCREMENT,
  `InventoryItemID` int NOT NULL,
  `IndividualItemID` int DEFAULT NULL,
  `StockID` int NOT NULL,
  `CenterID` int NOT NULL,
  `AppointmentID` int NOT NULL,
  `PatientID` int NOT NULL,
  `UsageDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `QuantityUsed` decimal(10,2) DEFAULT '1.00',
  `UsageNumber` int DEFAULT '1',
  `ItemCondition` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `Notes` text COLLATE utf8mb4_unicode_ci,
  `UsedBy` int NOT NULL,
  PRIMARY KEY (`UsageID`),
  KEY `InventoryItemID` (`InventoryItemID`),
  KEY `StockID` (`StockID`),
  KEY `CenterID` (`CenterID`),
  KEY `PatientID` (`PatientID`),
  KEY `idx_appointment` (`AppointmentID`),
  KEY `idx_individual_item` (`IndividualItemID`),
  KEY `idx_usage_date` (`UsageDate`),
  CONSTRAINT `t_inventory_usage_ibfk_1` FOREIGN KEY (`InventoryItemID`) REFERENCES `m_inventory_items` (`InventoryItemID`),
  CONSTRAINT `t_inventory_usage_ibfk_2` FOREIGN KEY (`IndividualItemID`) REFERENCES `t_inventory_individual_items` (`IndividualItemID`),
  CONSTRAINT `t_inventory_usage_ibfk_3` FOREIGN KEY (`StockID`) REFERENCES `t_inventory_stock` (`StockID`),
  CONSTRAINT `t_inventory_usage_ibfk_4` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`),
  CONSTRAINT `t_inventory_usage_ibfk_5` FOREIGN KEY (`AppointmentID`) REFERENCES `t_appointments` (`AppointmentID`),
  CONSTRAINT `t_inventory_usage_ibfk_6` FOREIGN KEY (`PatientID`) REFERENCES `m_patients` (`PatientID`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_inventory_usage`
--

LOCK TABLES `t_inventory_usage` WRITE;
/*!40000 ALTER TABLE `t_inventory_usage` DISABLE KEYS */;
INSERT INTO `t_inventory_usage` VALUES (1,2,NULL,13,1,29,4,'2026-02-09 21:25:10',1.00,1,NULL,NULL,0);
/*!40000 ALTER TABLE `t_inventory_usage` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_patientcycles`
--

DROP TABLE IF EXISTS `t_patientcycles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_patientcycles` (
  `CycleHistoryID` int NOT NULL AUTO_INCREMENT,
  `PatientID` int NOT NULL,
  `CycleNumber` int NOT NULL,
  `CycleStartDate` date NOT NULL,
  `CycleEndDate` date NOT NULL,
  `PlannedSessions` int DEFAULT '18',
  `CompletedSessions` int DEFAULT '0',
  `CycleStatus` varchar(20) DEFAULT 'Active',
  `FirstAppointmentDate` date DEFAULT NULL,
  `LastAppointmentDate` date DEFAULT NULL,
  `CreatedDate` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `CompletedDate` datetime DEFAULT NULL,
  PRIMARY KEY (`CycleHistoryID`),
  KEY `idx_patient_cycle` (`PatientID`,`CycleNumber`),
  KEY `idx_cycle_status` (`CycleStatus`),
  CONSTRAINT `t_patientcycles_ibfk_1` FOREIGN KEY (`PatientID`) REFERENCES `m_patients` (`PatientID`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_patientcycles`
--

LOCK TABLES `t_patientcycles` WRITE;
/*!40000 ALTER TABLE `t_patientcycles` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_patientcycles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_session_complications`
--

DROP TABLE IF EXISTS `t_session_complications`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_session_complications` (
  `ComplicationID` int NOT NULL AUTO_INCREMENT,
  `SessionID` int NOT NULL,
  `ComplicationType` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `Severity` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `OccurredAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `ResolvedAt` datetime DEFAULT NULL,
  `Description` text COLLATE utf8mb4_unicode_ci,
  `ActionTaken` text COLLATE utf8mb4_unicode_ci,
  `ReportedBy` int NOT NULL,
  PRIMARY KEY (`ComplicationID`),
  KEY `idx_session` (`SessionID`),
  KEY `idx_occurred` (`OccurredAt`),
  CONSTRAINT `t_session_complications_ibfk_1` FOREIGN KEY (`SessionID`) REFERENCES `t_dialysis_sessions` (`SessionID`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_session_complications`
--

LOCK TABLES `t_session_complications` WRITE;
/*!40000 ALTER TABLE `t_session_complications` DISABLE KEYS */;
/*!40000 ALTER TABLE `t_session_complications` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_session_inventory`
--

DROP TABLE IF EXISTS `t_session_inventory`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_session_inventory` (
  `SessionInventoryID` int NOT NULL AUTO_INCREMENT,
  `SessionID` int NOT NULL,
  `InventoryItemID` int NOT NULL,
  `IndividualItemID` int DEFAULT NULL,
  `StockID` int NOT NULL,
  `QuantityUsed` decimal(10,2) DEFAULT '1.00',
  `ItemCondition` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `UsageNumber` int DEFAULT NULL,
  `Notes` text COLLATE utf8mb4_unicode_ci,
  `SelectedAt` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `SelectedBy` int NOT NULL,
  PRIMARY KEY (`SessionInventoryID`),
  KEY `IndividualItemID` (`IndividualItemID`),
  KEY `StockID` (`StockID`),
  KEY `idx_session` (`SessionID`),
  KEY `idx_item` (`InventoryItemID`),
  CONSTRAINT `t_session_inventory_ibfk_1` FOREIGN KEY (`SessionID`) REFERENCES `t_dialysis_sessions` (`SessionID`) ON DELETE CASCADE,
  CONSTRAINT `t_session_inventory_ibfk_2` FOREIGN KEY (`InventoryItemID`) REFERENCES `m_inventory_items` (`InventoryItemID`),
  CONSTRAINT `t_session_inventory_ibfk_3` FOREIGN KEY (`IndividualItemID`) REFERENCES `t_inventory_individual_items` (`IndividualItemID`),
  CONSTRAINT `t_session_inventory_ibfk_4` FOREIGN KEY (`StockID`) REFERENCES `t_inventory_stock` (`StockID`)
) ENGINE=InnoDB AUTO_INCREMENT=17 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_session_inventory`
--

LOCK TABLES `t_session_inventory` WRITE;
/*!40000 ALTER TABLE `t_session_inventory` DISABLE KEYS */;
INSERT INTO `t_session_inventory` VALUES (1,10,1,1,12,1.00,'Good',NULL,NULL,'2026-01-21 22:51:44',0),(2,11,1,2,12,1.00,'Good',NULL,NULL,'2026-01-23 22:51:04',0),(3,11,2,NULL,13,1.00,NULL,NULL,NULL,'2026-01-23 22:51:05',0),(4,21,2,NULL,13,1.00,NULL,NULL,NULL,'2026-02-07 19:52:07',1),(5,23,2,NULL,13,1.00,NULL,NULL,NULL,'2026-02-09 07:41:00',1),(7,24,2,NULL,13,1.00,NULL,NULL,NULL,'2026-02-09 07:41:39',1),(8,25,2,NULL,13,1.00,NULL,NULL,NULL,'2026-02-09 21:24:28',1);
/*!40000 ALTER TABLE `t_session_inventory` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_session_notes`
--

DROP TABLE IF EXISTS `t_session_notes`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_session_notes` (
  `SessionNoteID` int NOT NULL AUTO_INCREMENT,
  `SessionID` int NOT NULL,
  `NoteTypeID` int NOT NULL,
  `NoteValue` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `NoteTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `IsAbnormal` bit(1) DEFAULT b'0',
  `AlertGenerated` bit(1) DEFAULT b'0',
  `RecordedBy` int NOT NULL,
  `Notes` text COLLATE utf8mb4_unicode_ci,
  PRIMARY KEY (`SessionNoteID`),
  KEY `idx_session` (`SessionID`),
  KEY `idx_note_type` (`NoteTypeID`),
  KEY `idx_note_time` (`NoteTime`),
  CONSTRAINT `t_session_notes_ibfk_1` FOREIGN KEY (`SessionID`) REFERENCES `t_dialysis_sessions` (`SessionID`) ON DELETE CASCADE,
  CONSTRAINT `t_session_notes_ibfk_2` FOREIGN KEY (`NoteTypeID`) REFERENCES `m_session_note_types` (`NoteTypeID`)
) ENGINE=InnoDB AUTO_INCREMENT=79 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_session_notes`
--

LOCK TABLES `t_session_notes` WRITE;
/*!40000 ALTER TABLE `t_session_notes` DISABLE KEYS */;
INSERT INTO `t_session_notes` VALUES (1,13,1,'90','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,'Htbr '),(2,13,2,'55','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,' Dbf '),(3,13,3,'160','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,' D d'),(4,13,4,'90','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,' D d '),(5,13,6,'60','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,' X d '),(6,13,7,'60','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,'Cb '),(7,13,8,'60','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,'C f c'),(8,13,15,'60','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,' C f '),(9,12,1,'120','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(10,12,2,'80','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(11,12,3,'70','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(12,12,4,'98','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(13,12,6,'100','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(14,12,7,'42','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(15,12,8,'40','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(16,12,15,'60','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(17,12,5,'96','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(18,12,9,'300','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(19,12,10,'500','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(20,12,11,'2','2026-01-23 18:37:24',_binary '\0',_binary '\0',0,NULL),(21,14,1,'150','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(22,14,2,'90','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(23,14,3,'90','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(24,14,4,'98','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(25,14,6,'102','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(26,14,7,'50','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(27,14,8,'47','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(28,14,15,'57','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(29,14,5,'96','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(30,14,9,'250','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(31,14,10,'500','2026-01-23 18:38:49',_binary '\0',_binary '\0',0,NULL),(32,11,1,'70','2026-01-23 22:52:46',_binary '\0',_binary '\0',0,'ghfgh'),(33,11,2,'40','2026-01-23 22:52:46',_binary '\0',_binary '\0',0,'gfhgdhf'),(34,11,3,'40','2026-01-23 22:52:46',_binary '\0',_binary '\0',0,'cvbc'),(35,11,4,'78','2026-01-23 22:52:46',_binary '\0',_binary '\0',0,'jghjghj'),(36,11,6,'50','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(37,11,7,'23','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(38,11,8,'23','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(39,11,15,'56','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,'dfgdf'),(40,11,5,'95','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,'gjfg'),(41,11,9,'200','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(42,11,10,'500','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(43,11,11,'5','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(44,11,12,'67','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(45,11,13,'67','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,NULL),(46,11,14,'67','2026-01-23 22:52:47',_binary '\0',_binary '\0',0,'ghf'),(47,15,1,'150','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(48,15,2,'90','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(49,15,3,'99','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(50,15,4,'98','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(51,15,6,'108','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(52,15,7,'78','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(53,15,8,'77','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(54,15,15,'58','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(55,15,12,'Stable ','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(56,15,13,'No','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(57,15,5,'96','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(58,15,9,'250','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(59,15,10,'550','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(60,15,14,'No','2026-01-24 11:50:46',_binary '\0',_binary '\0',0,NULL),(61,21,1,'72','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(62,21,2,'45','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(63,21,3,'45','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(64,21,4,'70','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(65,21,6,'52','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(66,21,7,'23','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(67,21,8,'23','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(68,21,15,'56','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(69,21,5,'95','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(70,21,9,'208','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(71,21,10,'520','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(72,21,11,'2','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(73,21,12,'Good','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(74,21,13,'No','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(75,21,16,'55','2026-02-07 23:47:17',_binary '\0',_binary '\0',0,NULL),(76,24,1,'70','2026-02-09 20:58:26',_binary '\0',_binary '\0',0,NULL),(77,23,1,'70','2026-02-09 20:58:39',_binary '\0',_binary '\0',0,NULL),(78,25,1,'70','2026-02-09 21:25:08',_binary '\0',_binary '\0',0,NULL);
/*!40000 ALTER TABLE `t_session_notes` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_session_timeline`
--

DROP TABLE IF EXISTS `t_session_timeline`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_session_timeline` (
  `TimelineID` int NOT NULL AUTO_INCREMENT,
  `SessionID` int NOT NULL,
  `EventType` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `EventDescription` text COLLATE utf8mb4_unicode_ci,
  `EventTime` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `PerformedBy` int NOT NULL,
  PRIMARY KEY (`TimelineID`),
  KEY `idx_session` (`SessionID`),
  KEY `idx_event_time` (`EventTime`),
  CONSTRAINT `t_session_timeline_ibfk_1` FOREIGN KEY (`SessionID`) REFERENCES `t_dialysis_sessions` (`SessionID`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=131 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_session_timeline`
--

LOCK TABLES `t_session_timeline` WRITE;
/*!40000 ALTER TABLE `t_session_timeline` DISABLE KEYS */;
INSERT INTO `t_session_timeline` VALUES (1,10,'SessionCreated','Dialysis session created','2026-01-21 22:51:02',0),(2,10,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-21 22:51:31',0),(3,10,'InventoryAdded','Item added: Dialyser (Qty: 1)','2026-01-21 22:51:44',0),(4,11,'SessionCreated','Dialysis session created','2026-01-22 21:07:32',0),(5,11,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-22 21:08:33',0),(6,12,'SessionCreated','Dialysis session created','2026-01-23 18:32:54',0),(7,13,'SessionCreated','Dialysis session created','2026-01-23 18:33:37',0),(8,14,'SessionCreated','Dialysis session created','2026-01-23 18:33:45',0),(9,13,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-23 18:35:11',0),(10,12,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-23 18:35:39',0),(11,14,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-23 18:36:27',0),(12,13,'NoteAdded','Blood Pressure (Systolic): 90 mmHg','2026-01-23 18:37:24',0),(13,13,'NoteAdded','Blood Pressure (Diastolic): 55 mmHg','2026-01-23 18:37:24',0),(14,13,'NoteAdded','Pulse Rate: 160 bpm','2026-01-23 18:37:24',0),(15,13,'NoteAdded','SpO2 (Oxygen Saturation): 90 %','2026-01-23 18:37:24',0),(16,13,'NoteAdded','Blood Sugar: 60 mg/dL','2026-01-23 18:37:24',0),(17,13,'NoteAdded','Weight (Pre-Dialysis): 60 kg','2026-01-23 18:37:24',0),(18,13,'NoteAdded','Weight (Post-Dialysis): 60 kg','2026-01-23 18:37:24',0),(19,13,'NoteAdded','Testv: 60','2026-01-23 18:37:24',0),(20,12,'NoteAdded','Blood Pressure (Systolic): 120 mmHg','2026-01-23 18:37:24',0),(21,12,'NoteAdded','Blood Pressure (Diastolic): 80 mmHg','2026-01-23 18:37:24',0),(22,12,'NoteAdded','Pulse Rate: 70 bpm','2026-01-23 18:37:24',0),(23,12,'NoteAdded','SpO2 (Oxygen Saturation): 98 %','2026-01-23 18:37:24',0),(24,12,'NoteAdded','Blood Sugar: 100 mg/dL','2026-01-23 18:37:24',0),(25,12,'NoteAdded','Weight (Pre-Dialysis): 42 kg','2026-01-23 18:37:24',0),(26,12,'NoteAdded','Weight (Post-Dialysis): 40 kg','2026-01-23 18:37:24',0),(27,12,'NoteAdded','Testv: 60','2026-01-23 18:37:24',0),(28,12,'NoteAdded','Temperature: 96 °F','2026-01-23 18:37:24',0),(29,12,'NoteAdded','Blood Flow Rate: 300 mL/min','2026-01-23 18:37:24',0),(30,12,'NoteAdded','Dialysate Flow Rate: 500 mL/min','2026-01-23 18:37:24',0),(31,12,'NoteAdded','Ultrafiltration Goal: 2 L','2026-01-23 18:37:24',0),(32,14,'NoteAdded','Blood Pressure (Systolic): 150 mmHg','2026-01-23 18:38:49',0),(33,14,'NoteAdded','Blood Pressure (Diastolic): 90 mmHg','2026-01-23 18:38:49',0),(34,14,'NoteAdded','Pulse Rate: 90 bpm','2026-01-23 18:38:49',0),(35,14,'NoteAdded','SpO2 (Oxygen Saturation): 98 %','2026-01-23 18:38:49',0),(36,14,'NoteAdded','Blood Sugar: 102 mg/dL','2026-01-23 18:38:49',0),(37,14,'NoteAdded','Weight (Pre-Dialysis): 50 kg','2026-01-23 18:38:49',0),(38,14,'NoteAdded','Weight (Post-Dialysis): 47 kg','2026-01-23 18:38:49',0),(39,14,'NoteAdded','Testv: 57','2026-01-23 18:38:49',0),(40,14,'NoteAdded','Temperature: 96 °F','2026-01-23 18:38:49',0),(41,14,'NoteAdded','Blood Flow Rate: 250 mL/min','2026-01-23 18:38:49',0),(42,14,'NoteAdded','Dialysate Flow Rate: 500 mL/min','2026-01-23 18:38:49',0),(43,11,'InventoryAdded','Item added: Dialyser (Qty: 1)','2026-01-23 22:51:04',0),(44,11,'InventoryAdded','Item added: Needles (Qty: 1)','2026-01-23 22:51:05',0),(45,11,'NoteAdded','Blood Pressure (Systolic): 70 mmHg','2026-01-23 22:52:46',0),(46,11,'NoteAdded','Blood Pressure (Diastolic): 40 mmHg','2026-01-23 22:52:46',0),(47,11,'NoteAdded','Pulse Rate: 40 bpm','2026-01-23 22:52:46',0),(48,11,'NoteAdded','SpO2 (Oxygen Saturation): 78 %','2026-01-23 22:52:46',0),(49,11,'NoteAdded','Blood Sugar: 50 mg/dL','2026-01-23 22:52:47',0),(50,11,'NoteAdded','Weight (Pre-Dialysis): 23 kg','2026-01-23 22:52:47',0),(51,11,'NoteAdded','Weight (Post-Dialysis): 23 kg','2026-01-23 22:52:47',0),(52,11,'NoteAdded','Testv: 56','2026-01-23 22:52:47',0),(53,11,'NoteAdded','Temperature: 95 °F','2026-01-23 22:52:47',0),(54,11,'NoteAdded','Blood Flow Rate: 200 mL/min','2026-01-23 22:52:47',0),(55,11,'NoteAdded','Dialysate Flow Rate: 500 mL/min','2026-01-23 22:52:47',0),(56,11,'NoteAdded','Ultrafiltration Goal: 5 L','2026-01-23 22:52:47',0),(57,11,'NoteAdded','Patient Condition: 67','2026-01-23 22:52:47',0),(58,11,'NoteAdded','Complications: 67','2026-01-23 22:52:47',0),(59,11,'NoteAdded','Other Notes: 67','2026-01-23 22:52:47',0),(60,11,'SessionStarted','Dialysis session started','2026-01-23 22:52:54',0),(61,11,'SessionCompleted','Dialysis session completed successfully','2026-01-23 22:53:42',0),(62,15,'SessionCreated','Dialysis session created','2026-01-24 11:46:50',0),(63,15,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-24 11:48:14',0),(64,15,'NoteAdded','Blood Pressure (Systolic): 150 mmHg','2026-01-24 11:50:46',0),(65,15,'NoteAdded','Blood Pressure (Diastolic): 90 mmHg','2026-01-24 11:50:46',0),(66,15,'NoteAdded','Pulse Rate: 99 bpm','2026-01-24 11:50:46',0),(67,15,'NoteAdded','SpO2 (Oxygen Saturation): 98 %','2026-01-24 11:50:46',0),(68,15,'NoteAdded','Blood Sugar: 108 mg/dL','2026-01-24 11:50:46',0),(69,15,'NoteAdded','Weight (Pre-Dialysis): 78 kg','2026-01-24 11:50:46',0),(70,15,'NoteAdded','Weight (Post-Dialysis): 77 kg','2026-01-24 11:50:46',0),(71,15,'NoteAdded','Testv: 58','2026-01-24 11:50:46',0),(72,15,'NoteAdded','Patient Condition: Stable ','2026-01-24 11:50:46',0),(73,15,'NoteAdded','Complications: No','2026-01-24 11:50:46',0),(74,15,'NoteAdded','Temperature: 96 °F','2026-01-24 11:50:46',0),(75,15,'NoteAdded','Blood Flow Rate: 250 mL/min','2026-01-24 11:50:46',0),(76,15,'NoteAdded','Dialysate Flow Rate: 550 mL/min','2026-01-24 11:50:46',0),(77,15,'NoteAdded','Other Notes: No','2026-01-24 11:50:46',0),(78,16,'SessionCreated','Dialysis session created','2026-01-24 20:30:27',0),(79,16,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-24 20:55:49',0),(80,17,'SessionCreated','Dialysis session created','2026-01-25 10:32:51',1),(81,17,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-25 10:32:52',1),(82,18,'SessionCreated','Dialysis session created','2026-01-27 16:37:56',1),(83,18,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-01-27 16:37:59',1),(84,19,'SessionCreated','Dialysis session created','2026-02-04 16:16:30',1),(85,19,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-02-04 16:16:30',1),(86,20,'SessionCreated','Dialysis session created','2026-02-05 08:35:33',1),(87,20,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-02-05 08:35:33',1),(88,21,'SessionCreated','Dialysis session created','2026-02-07 19:51:49',1),(89,21,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-02-07 19:51:50',1),(90,21,'InventoryAdded','Item added: Needles (Qty: 1)','2026-02-07 19:52:07',1),(91,21,'SessionStarted','Dialysis session started','2026-02-07 19:52:10',1),(92,22,'SessionCreated','Dialysis session created','2026-02-07 19:54:17',1),(93,21,'NoteAdded','Blood Pressure (Systolic): 72 mmHg','2026-02-07 23:47:17',0),(94,21,'NoteAdded','Blood Pressure (Diastolic): 45 mmHg','2026-02-07 23:47:17',0),(95,21,'NoteAdded','Pulse Rate: 45 bpm','2026-02-07 23:47:17',0),(96,21,'NoteAdded','SpO2 (Oxygen Saturation): 70 %','2026-02-07 23:47:17',0),(97,21,'NoteAdded','Blood Sugar: 52 mg/dL','2026-02-07 23:47:17',0),(98,21,'NoteAdded','Weight (Pre-Dialysis): 23 kg','2026-02-07 23:47:17',0),(99,21,'NoteAdded','Weight (Post-Dialysis): 23 kg','2026-02-07 23:47:17',0),(100,21,'NoteAdded','Testv: 56','2026-02-07 23:47:17',0),(101,21,'NoteAdded','Temperature: 95 °F','2026-02-07 23:47:17',0),(102,21,'NoteAdded','Blood Flow Rate: 208 mL/min','2026-02-07 23:47:17',0),(103,21,'NoteAdded','Dialysate Flow Rate: 520 mL/min','2026-02-07 23:47:17',0),(104,21,'NoteAdded','Ultrafiltration Goal: 2 L','2026-02-07 23:47:17',0),(105,21,'NoteAdded','Patient Condition: Good','2026-02-07 23:47:17',0),(106,21,'NoteAdded','Complications: No','2026-02-07 23:47:17',0),(107,21,'NoteAdded','Venous Pressure: 55 mmHg','2026-02-07 23:47:17',0),(108,21,'SessionCompleted','Dialysis session completed successfully','2026-02-07 23:47:24',0),(109,23,'SessionCreated','Dialysis session created','2026-02-09 07:40:49',1),(110,23,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-02-09 07:40:49',1),(111,23,'InventoryAdded','Item added: Needles (Qty: 1)','2026-02-09 07:41:00',1),(112,23,'SessionStarted','Dialysis session started','2026-02-09 07:41:11',1),(113,24,'SessionCreated','Dialysis session created','2026-02-09 07:41:33',1),(114,24,'MachineAssigned','Dialysis machine DM-NEP-0002 assigned','2026-02-09 07:41:34',1),(115,24,'InventoryAdded','Item added: Needles (Qty: 1)','2026-02-09 07:41:39',1),(116,24,'SessionStarted','Dialysis session started','2026-02-09 07:41:41',1),(117,24,'NoteAdded','Blood Pressure (Systolic): 70 mmHg','2026-02-09 20:58:26',0),(118,24,'SessionCompleted','Dialysis session completed successfully','2026-02-09 20:58:30',0),(119,23,'NoteAdded','Blood Pressure (Systolic): 70 mmHg','2026-02-09 20:58:39',0),(120,23,'SessionCompleted','Dialysis session completed successfully','2026-02-09 20:58:45',0),(121,25,'SessionCreated','Dialysis session created','2026-02-09 21:24:22',1),(122,25,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-02-09 21:24:22',1),(123,25,'InventoryAdded','Item added: Needles (Qty: 1)','2026-02-09 21:24:28',1),(124,25,'SessionStarted','Dialysis session started','2026-02-09 21:24:33',1),(125,25,'NoteAdded','Blood Pressure (Systolic): 70 mmHg','2026-02-09 21:25:08',0),(126,25,'SessionCompleted','Dialysis session completed successfully','2026-02-09 21:25:10',0),(127,26,'SessionCreated','Dialysis session created','2026-02-09 22:02:25',1),(128,26,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-02-09 22:02:25',1),(129,27,'SessionCreated','Dialysis session created','2026-02-10 08:37:35',1),(130,27,'MachineAssigned','Dialysis machine DM-NEP-0001 assigned','2026-02-10 08:37:35',1);
/*!40000 ALTER TABLE `t_session_timeline` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `t_slots`
--

DROP TABLE IF EXISTS `t_slots`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `t_slots` (
  `SlotID` int NOT NULL AUTO_INCREMENT,
  `AppointmentID` int NOT NULL,
  `PatientID` int NOT NULL,
  `CenterID` int NOT NULL,
  `CompanyID` int NOT NULL,
  `SlotStartTime` time NOT NULL,
  `SlotEndTime` time NOT NULL,
  `SlotDate` date NOT NULL,
  `IsActive` bit(1) DEFAULT b'1',
  PRIMARY KEY (`SlotID`),
  KEY `AppointmentID` (`AppointmentID`),
  KEY `PatientID` (`PatientID`),
  KEY `CompanyID` (`CompanyID`),
  KEY `idx_slot_date_time` (`SlotDate`,`SlotStartTime`,`SlotEndTime`),
  KEY `idx_center_date` (`CenterID`,`SlotDate`),
  CONSTRAINT `t_slots_ibfk_1` FOREIGN KEY (`AppointmentID`) REFERENCES `t_appointments` (`AppointmentID`) ON DELETE CASCADE,
  CONSTRAINT `t_slots_ibfk_2` FOREIGN KEY (`PatientID`) REFERENCES `m_patients` (`PatientID`),
  CONSTRAINT `t_slots_ibfk_3` FOREIGN KEY (`CenterID`) REFERENCES `m_centers` (`CenterID`),
  CONSTRAINT `t_slots_ibfk_4` FOREIGN KEY (`CompanyID`) REFERENCES `m_companies` (`CompanyID`)
) ENGINE=InnoDB AUTO_INCREMENT=37 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `t_slots`
--

LOCK TABLES `t_slots` WRITE;
/*!40000 ALTER TABLE `t_slots` DISABLE KEYS */;
INSERT INTO `t_slots` VALUES (1,1,1,1,1,'08:00:00','12:00:00','2026-01-16',_binary ''),(2,2,1,1,1,'12:00:00','16:00:00','2026-01-17',_binary ''),(3,3,2,1,1,'08:00:00','12:00:00','2026-01-18',_binary '\0'),(4,4,2,1,1,'12:00:00','16:00:00','2026-01-18',_binary '\0'),(5,5,2,1,1,'12:00:00','16:00:00','2026-01-18',_binary '\0'),(6,6,1,1,1,'08:00:00','12:00:00','2026-01-20',_binary ''),(7,7,1,1,1,'11:00:00','12:00:00','2026-01-19',_binary ''),(8,8,2,1,1,'12:00:00','13:00:00','2026-01-20',_binary ''),(9,9,3,1,1,'23:00:00','00:00:00','2026-01-20',_binary ''),(10,10,4,1,1,'14:00:00','15:00:00','2026-01-21',_binary ''),(11,11,5,1,1,'10:00:00','11:00:00','2026-01-23',_binary ''),(12,12,7,1,1,'08:00:00','09:00:00','2026-01-24',_binary ''),(13,13,6,1,1,'11:00:00','12:00:00','2026-01-24',_binary ''),(14,14,8,1,1,'23:00:00','00:00:00','2026-01-24',_binary '\0'),(15,14,8,1,1,'23:00:00','00:00:00','2026-01-24',_binary '\0'),(16,14,8,1,1,'23:00:00','00:00:00','2026-01-24',_binary '\0'),(17,15,8,1,1,'09:00:00','10:00:00','2026-01-24',_binary '\0'),(18,15,8,1,1,'08:00:00','09:00:00','2026-01-27',_binary '\0'),(19,15,8,1,1,'23:00:00','00:00:00','2026-01-23',_binary '\0'),(20,15,8,1,1,'23:00:00','00:00:00','2026-01-23',_binary ''),(21,16,1,1,1,'09:00:00','10:00:00','2026-01-24',_binary ''),(22,17,10,1,1,'12:00:00','13:00:00','2026-01-24',_binary ''),(23,18,1,1,1,'08:00:00','09:00:00','2026-01-25',_binary ''),(24,19,2,1,1,'08:00:00','09:00:00','2026-01-25',_binary ''),(25,20,1,1,1,'17:00:00','18:00:00','2026-01-27',_binary ''),(26,21,2,1,1,'17:00:00','18:00:00','2026-01-27',_binary ''),(27,22,4,1,1,'14:00:00','15:00:00','2026-02-04',_binary ''),(28,23,1,1,1,'22:00:00','23:00:00','2026-02-04',_binary ''),(29,24,1,1,1,'09:00:00','10:00:00','2026-02-05',_binary ''),(30,25,1,1,1,'20:00:00','21:00:00','2026-02-07',_binary ''),(31,26,2,1,1,'20:00:00','21:00:00','2026-02-07',_binary ''),(32,27,1,1,1,'08:00:00','09:00:00','2026-02-09',_binary ''),(33,28,2,1,1,'08:00:00','09:00:00','2026-02-09',_binary ''),(34,29,4,1,1,'22:00:00','23:00:00','2026-02-09',_binary ''),(35,30,8,1,1,'23:00:00','00:00:00','2026-02-09',_binary ''),(36,31,1,1,1,'09:00:00','10:00:00','2026-02-10',_binary '');
/*!40000 ALTER TABLE `t_slots` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping routines for database 'dms_db'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2026-02-10  8:51:22
