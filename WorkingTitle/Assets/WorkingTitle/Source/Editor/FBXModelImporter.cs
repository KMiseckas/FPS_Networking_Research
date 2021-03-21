using UnityEditor;

public class FBXModelImporter : AssetPostprocessor
{
    #region Functions

    /// <summary>
    ///     Disable import of materials
    /// </summary>
    public void OnPreprocessModel()
    {
        ModelImporter importer = assetImporter as ModelImporter;
        importer.materialImportMode = ModelImporterMaterialImportMode.None;
    }

    #endregion
}
