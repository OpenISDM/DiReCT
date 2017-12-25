namespace DiReCT_Record
{
    #region RecordOfDebrisFlow
    public enum EnumCatchmentLandslideScale
    {
        SmallScale = 0,
        MediumScale,
        LargeScale,
        IDoNotKnow
    }

    public enum EnumCatchmentPictureDirection
    {
        North = 0,
        NorthWest,
        NorthEast,
        West,
        East,
        South,
        SouthWest,
        SouthEast,
        IDoNotKnow
    }

    public enum EnumRockType
    {
        TypesOfSedimetaryRock = 0,
        Conglomerate,
        Sandstone,
        Siltstone,
        Shale,
        Mudstone,
        Limestone,
        ///    Types of Metamorphic Rock.
        Quartzite,
        Marble,
        Amphibolite,
        Gneiss,
        GraniticGneiss,
        Schist,
        Phyllite,
        Slate,
        Hornfels,
        Greywacke,
        Argillite,
        ///    Types of Igneous Rock.
        Peridotite,
        Gabbro,
        Diorite,
        Granite,
        Granodiorite,
        Basalt,
        Andesite,
        Rhyolite,
        VolcanicGlass,
        QuartzVein,
        Agglomerate,
        Ignimbrite,
        Tuff,
        Lahar,
        ///    Types of Sedoment Rock.
        GarvelTerrace,
        ClayLayer,
        Peat,
        Lapilli,
        VolcanicAsh,
        IDoNotKnow
    }

    public enum EnumRockPictureDirection
    {
        North,
        NorthWest,
        NorthEast,
        West,
        East,
        South,
        SouthWest,
        SouthEast,
        IDoNotKnow
    }

    public enum EnumPlantationCategory
    {
        Naked,
        Meadow,
        ArtificialForest,
        NaturalForest,
        IDoNotKnow
    }

    public enum EnumPlantationSituation
    {
        BareLend,
        UnderTenPercents,
        TenToThirtyPercents,
        ThirtyToEightyPercents,
        AboveEightyPercents,
    }

    public enum EnumPlantationPictureDirection
    {
        North,
        NorthWest,
        NorthEast,
        West,
        East,
        South,
        SouthWest,
        SouthEast,
        IDoNotKnow
    }

    public enum EnumSlopeDirection
    {
        North,
        NorthWest,
        NorthEast,
        West,
        East,
        South,
        SouthWest,
        SouthEast,
        IDoNotKnow
    }

    public enum EnumSlopePictureDirection
    {
        North,
        NorthWest,
        NorthEast,
        West,
        East,
        South,
        SouthWest,
        SouthEast,
        IDoNotKnow
    }

    /// <summary>
    /// 集水區相關
    /// The Catchment struct is used to store "Catchment" related 
    /// properties that are included in observation record on a 
    /// debris flow event.
    /// </summary>
    public class Catchment
    {
        /// <summary>
        /// 集水區內崩塌規模類型
        /// This member stores the catchment scale of landslide.
        /// enum: 
        ///     1. Small scale
        ///     2. Medium scale
        ///     3. Large scale
        ///     4. I don't know
        /// </summary>
        public int CatchmentLandslideScale{ get;set; }

        /// <summary>
        /// 集水區照片方位
        /// This member stores the direction of the catchment picture.
        /// enum:
        ///     1. North
        ///     2. NorthWest
        ///     3. NorthEast
        ///     4. West
        ///     5. East
        ///     6. South
        ///     7. SouthWest
        ///     8. SouthEast
        ///     9. I don't know
        /// </summary>
        public int CatchmentPictureDirection{ get;set; }

        public Catchment(EnumCatchmentLandslideScale catchmentLandslideScale,
            EnumCatchmentPictureDirection catchmentPictureDirection)
        {
            CatchmentLandslideScale = (int)catchmentLandslideScale;
            CatchmentPictureDirection = (int)catchmentPictureDirection;
        }

        public Catchment()
        {

        }
    }

    /// <summary>
    /// 岩石紀錄
    /// The Rock struct is used to store "Rock" related properties 
    /// that are included in observation record on a debris flow event.
    /// </summary>
    public class Rock
    {
        /// <summary>
        /// 土石種類
        /// This member stores the type of the rock.
        /// enum:
        ///    Types of Sedimetary Rock.
        ///    1. Conglomerate,
        ///    2. Sandstone,
        ///    3. Siltstone,
        ///    4. Shale,
        ///    5. Mudstone,
        ///    6. Limestone,
        ///    Types of Metamorphic Rock.
        ///    7. Quartzite,
        ///    8. Marble,
        ///    9. Amphibolite,
        ///    10. Gneiss,
        ///    11. GraniticGneiss,
        ///    12. Schist,
        ///    13. Phyllite,
        ///    14. Slate,
        ///    15. Hornfels,
        ///    16. Greywacke,
        ///    17. Argillite,
        ///    Types of Igneous Rock.
        ///    18. Peridotite,
        ///    19. Gabbro,
        ///    20. Diorite,
        ///    21. Granite,
        ///    22. Granodiorite,
        ///    23. Basalt,
        ///    24. Andesite,
        ///    25. Rhyolite,
        ///    26. VolcanicGlass,
        ///    27. QuartzVein,
        ///    28. Agglomerate,
        ///    29. Ignimbrite,
        ///    30. Tuff,
        ///    31. Lahar,
        ///    Types of Sedoment Rock.
        ///    32. GarvelTerrace,
        ///    33. ClayLayer,
        ///    34. Peat,
        ///    35. Lapilli,
        ///    36. VolcanicAsh,
        ///    37. I don't know
        /// </summary>
        public int RockType{ get;set; }

        /// <summary>
        /// 平均土石粒徑
        /// This member stores the average diameters of the rock.
        /// </summary>
        public int AverageRockDiameters{ get;set; }

        /// <summary>
        /// 土石照片方位
        /// This member stores the direction of the rock picture.
        /// enum:
        ///     1. North
        ///     2. NorthWest
        ///     3. NorthEast
        ///     4. West
        ///     5. East
        ///     6. South
        ///     7. SouthWest
        ///     8. SouthEast
        ///     9. I don't know
        /// </summary>
        public int RockPictureDirection{ get;set; }

        public Rock(EnumRockType rockType, int averageRockDiameters,
            EnumRockPictureDirection rockPictureDirection)
        {
            RockType = (int)rockType;
            AverageRockDiameters = averageRockDiameters;
            RockPictureDirection = (int)rockPictureDirection;
        }

        public Rock()
        {

        }
    }

    /// <summary>
    /// 植生相關
    /// The Plantation struct is used to store "Plantation" related 
    /// properties that are included in observation record on a 
    /// debris flow event.
    /// </summary>
    public class Plantation
    {
        /// <summary>
        /// 植生生長種類
        /// This member stores the type of the plantation.
        /// enum:
        ///     1. Naked
        ///     2. Meadow
        ///     3. ArtificialForest
        ///     4. NaturalForest    
        ///     5. I don't know
        /// </summary>
        public int PlantationCategory{ get;set; }

        /// <summary>
        /// 植生生長狀態類型
        /// This member stores the situation of the plantation.
        /// enum:
        ///     1. BareLend
        ///     2. UnderTenPercents,
        ///     3. TenToThirtyPercents,
        ///     4. ThirtyToEightyPercents,
        ///     5. AboveEightyPercents,
        /// </summary>
        public int PlantationSituation{ get;set; }

        /// <summary>
        /// 植生照片方位
        /// This member stores the direction of the plantation picture.
        /// enum:
        ///     1. North
        ///     2. NorthWest
        ///     3. NorthEast
        ///     4. West
        ///     5. East
        ///     6. South
        ///     7. SouthWest
        ///     8. SouthEast
        ///     9. I don't know
        /// </summary>
        public int PlantationPictureDirection{ get;set; }

        public Plantation(EnumPlantationCategory plantationCategory,
            EnumPlantationSituation plantationSituation,
            EnumPlantationPictureDirection plantationPictureDirection)
        {
            PlantationCategory = (int)plantationCategory;
            PlantationSituation = (int)plantationSituation;
            PlantationPictureDirection = (int)plantationPictureDirection;
        }

        public Plantation()
        {

        }
    }

    /// <summary>
    /// 坡地相關
    /// The Slope struct is used to store "Slope" related properties 
    /// that are included in observation record on a debris flow event.
    /// </summary>
    public class Slope
    {
        /// <summary>
        /// 坡地角度
        /// This member stores the angles of the slope.
        /// </summary>
        public int SlopeAngles{ get;set; }

        /// <summary>
        /// 坡地方向
        /// This member stores the direction of the slope.
        /// enum:
        ///     1. North
        ///     2. NorthWest
        ///     3. NorthEast
        ///     4. West
        ///     5. East
        ///     6. South
        ///     7. SouthWest
        ///     8. SouthEast
        ///     9. I don't know
        /// </summary>
        public int SlopeDirection{ get;set; }

        /// <summary>
        /// 坡地照片方位
        /// This member stores the direction of the slope picture.
        /// enum:
        ///     1. North
        ///     2. NorthWest
        ///     3. NorthEast
        ///     4. West
        ///     5. East
        ///     6. South
        ///     7. SouthWest
        ///     8. SouthEast
        ///     9. I don't know
        /// </summary>
        public int SlopePictureDirection{ get;set; }

        public Slope(int slopeAngles, EnumSlopeDirection slopeDirection,
            EnumSlopePictureDirection slopePictureDirection)
        {
            SlopeAngles = slopeAngles;
            SlopeDirection = (int)slopeDirection;
            SlopePictureDirection = (int)slopePictureDirection;
        }

        public Slope()
        {

        }
    }
    #endregion

}
