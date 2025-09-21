using Haven.Forms;
using Haven.Parser;
using Haven.Parser.Geom;
using Haven.Properties;
using Haven.Render;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Serilog;
using Serilog.Events;
using System.Diagnostics;

namespace Haven
{
    public partial class MainForm : Form
    {
        public Stage? CurrentStage;
        public GeomFile? Geom;

        public Scene Scene;

        public List<Mesh> MeshGroups = new List<Mesh>();
        public List<Mesh> MeshRefs = new List<Mesh>();
        public List<Mesh> MeshProps = new List<Mesh>();
        public List<Mesh> MeshBoundaries = new List<Mesh>();

        public Dictionary<GeomProp, Mesh> GeomPropMeshLookup = new Dictionary<GeomProp, Mesh>();
        public Dictionary<Mesh, GeomProp> MeshGeomPropLookup = new Dictionary<Mesh, GeomProp>();
        public Dictionary<TreeNode, StageFile> StageFileLookup = new Dictionary<TreeNode, StageFile>();
        public Dictionary<TreeNode, GeomProp> GeomPropLookup = new Dictionary<TreeNode, GeomProp>();
        public Dictionary<string, TreeNode> TreeNodeLookup = new();

        public ContextMenuStrip ContextMenuFiles = new ContextMenuStrip();
        public ToolStripMenuItem MenuItemFilesOpen = new ToolStripMenuItem();
        public ToolStripMenuItem MenuItemFilesEdit = new ToolStripMenuItem();
        public ToolStripMenuItem MenuItemFilesRebuild = new ToolStripMenuItem();

        public ContextMenuStrip ContextMenuGeomProp = new ContextMenuStrip();
        public ToolStripMenuItem MenuItemGeomPropEdit = new ToolStripMenuItem();

        public ContextMenuStrip ContextMenuGeomMesh = new ContextMenuStrip();
        public ToolStripMenuItem MenuItemGeomMeshEdit = new ToolStripMenuItem();

        public TreeNode TreeNodeGeomMeshes;
        public TreeNode TreeNodeGeomRefs;
        public TreeNode TreeNodeGeomProps;
        public TreeNode TreeNodeGeomBoundaries;

        public static string LoggerTemplate = "[{Timestamp:HH:mm:ss} {Level:u3} {SourceContext}] {Message:lj}{NewLine}{Exception}";
        public static CustomLoggerSink LoggerSink = new CustomLoggerSink(null, LoggerTemplate);

        private bool _suspendAfterCheck = false;



        public MainForm()
        {
            InitializeComponent();

            TreeNodeGeomMeshes = treeViewGeom.Nodes.Add("Meshes");
            TreeNodeGeomProps = treeViewGeom.Nodes.Add("Props");
            TreeNodeGeomRefs = treeViewGeom.Nodes.Add("References");
            TreeNodeGeomBoundaries = treeViewGeom.Nodes.Add("Boundaries");

            Scene = new Scene(glControl);

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(LogEventLevel.Verbose)
                .WriteTo.File("log.txt", outputTemplate: LoggerTemplate)
                .WriteTo.Sink(LoggerSink)
                .CreateLogger();

            LoggerSink.NewLogHandler += LoggerSink_NewLogHandler;
        }

        private ToolStripMenuItem texturesToolStripMenuItem;
        private ToolStripMenuItem dumpAllTxnsToolStripMenuItem;

        private GeoPropCategoryInfo GetPropCategoryInfo(string propId)
        {
            // --- Race Mission (RACE) ---
            if (propId.StartsWith("PRP_RES_01_MINI_RACE_HOME_") || propId.StartsWith("PRP_RES_02_MINI_RACE_HOME_") || propId.StartsWith("PRP_RES_03_MINI_RACE_HOME_") || propId.StartsWith("PRP_RES_04_MINI_RACE_HOME_"))
                return new GeoPropCategoryInfo("RACE", true);
            if (propId.StartsWith("PRP_MINI_RACE_HOME_")) return new GeoPropCategoryInfo("RACE", true);
            if (propId.StartsWith("PRP_RES_01_MINI_RACE_BASE_") || propId.StartsWith("PRP_RES_02_MINI_RACE_BASE_") || propId.StartsWith("PRP_RES_03_MINI_RACE_BASE_") || propId.StartsWith("PRP_RES_04_MINI_RACE_BASE_"))
                return new GeoPropCategoryInfo("RACE", true);
            if (propId.StartsWith("PRP_MINI_RACE_BASE_")) return new GeoPropCategoryInfo("RACE", true);
            if (propId.StartsWith("PRP_MINI_RACE_GOAL_")) return new GeoPropCategoryInfo("RACE", true);
            if (propId.StartsWith("PRP_MINI_RACE_TGT_")) return new GeoPropCategoryInfo("RACE", true);
            if (propId.StartsWith("PRP_MINI_RACE_A") || propId.StartsWith("PRP_MINI_RACE_B")) return new GeoPropCategoryInfo("RACE", true);

            if (propId.StartsWith("PRP_RES_01_RACE_HOME_") || propId.StartsWith("PRP_RES_02_RACE_HOME_") || propId.StartsWith("PRP_RES_03_RACE_HOME_") || propId.StartsWith("PRP_RES_04_RACE_HOME_"))
                return new GeoPropCategoryInfo("RACE", false);
            if (propId.StartsWith("PRP_RACE_HOME_")) return new GeoPropCategoryInfo("RACE", false);
            if (propId.StartsWith("PRP_RES_01_RACE_BASE_") || propId.StartsWith("PRP_RES_02_RACE_BASE_") || propId.StartsWith("PRP_RES_03_RACE_BASE_") || propId.StartsWith("PRP_RES_04_RACE_BASE_"))
                return new GeoPropCategoryInfo("RACE", false);
            if (propId.StartsWith("PRP_RACE_BASE_")) return new GeoPropCategoryInfo("RACE", false);
            if (propId.StartsWith("PRP_RACE_TGT_")) return new GeoPropCategoryInfo("RACE", false);
            if (propId.StartsWith("PRP_RACE_A") || propId.StartsWith("PRP_RACE_B")) return new GeoPropCategoryInfo("RACE", false);
            if (propId.StartsWith("PRP_RACE_")) return new GeoPropCategoryInfo("RACE", false);

            // --- Bomb Mission (BOMB) ---
            if (propId.StartsWith("PRP_RES_MINI_BOMB_")) return new GeoPropCategoryInfo("BOMB", true);
            if (propId.StartsWith("PRP_MINI_BOMB_SITE_")) return new GeoPropCategoryInfo("BOMB", true);
            if (propId.StartsWith("PRP_MINI_BOMB_TERMINAL_")) return new GeoPropCategoryInfo("BOMB", true);
            if (propId.StartsWith("PRP_MINI_BOMB_")) return new GeoPropCategoryInfo("BOMB", true);
            if (propId.StartsWith("PRP_RES_BOMB_")) return new GeoPropCategoryInfo("BOMB", false);
            if (propId.StartsWith("PRP_BOMB_SITE_")) return new GeoPropCategoryInfo("BOMB", false);
            if (propId.StartsWith("PRP_BOMB_TGT_")) return new GeoPropCategoryInfo("BOMB", false);
            if (propId.StartsWith("PRP_BOMB_TERMINAL_")) return new GeoPropCategoryInfo("BOMB", false);
            if (propId.StartsWith("PRP_BOMB_")) return new GeoPropCategoryInfo("BOMB", false);

            // --- Team Sneaking Mission (TSNE) ---
            if (propId.StartsWith("PRP_MINI_TEAM_SNEAKING_GOAL_")) return new GeoPropCategoryInfo("TSNE", true);
            if (propId.StartsWith("PRP_MINI_TEAM_SNEAKING_TGT_")) return new GeoPropCategoryInfo("TSNE", true);
            if (propId.StartsWith("PRP_MINI_TEAM_SNEAKING_")) return new GeoPropCategoryInfo("TSNE", true);
            if (propId.StartsWith("PRP_TEAM_SNEAKING_GOAL_")) return new GeoPropCategoryInfo("TSNE", false);
            if (propId.StartsWith("PRP_TEAM_SNEAKING_TGT_")) return new GeoPropCategoryInfo("TSNE", false);
            if (propId.StartsWith("PRP_TEAM_SNEAKING_")) return new GeoPropCategoryInfo("TSNE", false);

            // --- Stealth Deathmatch Mission (SDM) ---
            if (propId.StartsWith("PRP_SDM_CIRCLE_")) return new GeoPropCategoryInfo("SDM", false);
            if (propId.StartsWith("PRP_MINI_SDM_")) return new GeoPropCategoryInfo("SDM", true);
            if (propId.StartsWith("PRP_SDM_")) return new GeoPropCategoryInfo("SDM", false);

            // --- Solo Capture Mission (SCAP) ---
            if (propId.StartsWith("PRP_MINI_SCAP_TERMINAL")) return new GeoPropCategoryInfo("SCAP", true);
            if (propId.StartsWith("PRP_MINI_SCAP_")) return new GeoPropCategoryInfo("SCAP", true);
            if (propId.StartsWith("PRP_SCAP_TGT_")) return new GeoPropCategoryInfo("SCAP", false);
            if (propId.StartsWith("PRP_SCAP_TERMINAL")) return new GeoPropCategoryInfo("SCAP", false);
            if (propId.StartsWith("PRP_SCAP_")) return new GeoPropCategoryInfo("SCAP", false);

            // --- Rugby Mission (RUG) ---
            if (propId.StartsWith("PRP_RES_MINI_RUGBY_")) return new GeoPropCategoryInfo("CAP", true);
            if (propId.StartsWith("PRP_MINI_CAP_TERMINAL_")) return new GeoPropCategoryInfo("CAP", true);
            if (propId.StartsWith("PRP_MINI_RUGBY_")) return new GeoPropCategoryInfo("CAP", true);
            if (propId.StartsWith("PRP_RES_RUGBY_")) return new GeoPropCategoryInfo("CAP", false);
            if (propId.StartsWith("PRP_RUGBY_GOAL_")) return new GeoPropCategoryInfo("CAP", false);
            if (propId.StartsWith("PRP_RUGBY_TGT_")) return new GeoPropCategoryInfo("CAP", false);
            if (propId.StartsWith("PRP_CAP_TERMINAL_")) return new GeoPropCategoryInfo("CAP", false);
            if (propId.StartsWith("PRP_RUGBY_")) return new GeoPropCategoryInfo("CAP", false);

            // --- Team Deathmatch (TDM) ---
            if (propId.StartsWith("PRP_RES_MINI_TEAM_DEATHMATCH_")) return new GeoPropCategoryInfo("TDM", true);
            if (propId.StartsWith("PRP_MINI_TEAM_DEATHMATCH_")) return new GeoPropCategoryInfo("TDM", true);
            if (propId.StartsWith("PRP_RES_TEAM_DEATHMATCH_")) return new GeoPropCategoryInfo("TDM", false);
            if (propId.StartsWith("PRP_TEAM_DEATHMATCH_")) return new GeoPropCategoryInfo("TDM", false);

            // --- Deathmatch (DM) ---
            if (propId.StartsWith("PRP_MINI_DM_TERMINAL")) return new GeoPropCategoryInfo("DM", true);
            if (propId.StartsWith("PRP_MINI_DEATHMATCH_")) return new GeoPropCategoryInfo("DM", true);
            if (propId.StartsWith("PRP_DM_TERMINAL")) return new GeoPropCategoryInfo("DM", false);
            if (propId.StartsWith("PRP_DEATHMATCH_")) return new GeoPropCategoryInfo("DM", false);

            // --- Rescue Mission (RES Mission) ---
            if (propId.StartsWith("PRP_MINI_RESCUE_")) return new GeoPropCategoryInfo("RES", true);
            if (propId.StartsWith("PRP_MINI_RES_A") || propId.StartsWith("PRP_MINI_RES_B")) return new GeoPropCategoryInfo("RES", true);
            if (propId.StartsWith("PRP_RESCUE_")) return new GeoPropCategoryInfo("RES", false);
            if (propId.StartsWith("PRP_RES_GOAL_")) return new GeoPropCategoryInfo("RES", false);
            if (propId.StartsWith("PRP_RES_A") || propId.StartsWith("PRP_RES_B")) return new GeoPropCategoryInfo("RES", false);
            if (propId.StartsWith("PRP_RES_TGT_00")) return new GeoPropCategoryInfo("RES", false);
            if (propId.StartsWith("PRP_RES_TGT_01")) return new GeoPropCategoryInfo("RES", true);

            // --- Training Mission (TRA Mission) ---
            if (propId.StartsWith("PRP_TRAINING_")) return new GeoPropCategoryInfo("TRAIN", false);
            if (propId.StartsWith("PRP_DOLL_")) return new GeoPropCategoryInfo("TRAIN", false);
            if (propId.StartsWith("PRP_SLEEP_")) return new GeoPropCategoryInfo("TRAIN", false);
            if (propId.StartsWith("PRP_CLAYMORE_")) return new GeoPropCategoryInfo("TRAIN", false);
            if (propId.StartsWith("PRP_RES_TRAINING_")) return new GeoPropCategoryInfo("TRAIN", false);
            if (propId.StartsWith("PRP_MINI_TRAINING_")) return new GeoPropCategoryInfo("TRAIN", true);
            if (propId.StartsWith("PRP_RES_MINI_TRAINING_")) return new GeoPropCategoryInfo("TRAIN", true);

            // --- Explosive Barrel ---
            if (propId.StartsWith("PRP_EXP_BARREL_")) return new GeoPropCategoryInfo("Explosive Barrel", false);


            // --- Sneaking Mission (SNE Mission) ---
            if (propId.StartsWith("PRP_SNEAKING_")) return new GeoPropCategoryInfo("SNE", false);
            if (propId.StartsWith("PRP_MINI_SNEAKING_")) return new GeoPropCategoryInfo("SNE", true);


            // --- Combat Training (CBT Mission) ---
            if (propId.StartsWith("PRP_CBTRAIN_")) return new GeoPropCategoryInfo("CBTRAIN", false);
            if (propId.StartsWith("PRP_RES_CBTRAIN_")) return new GeoPropCategoryInfo("CBTRAIN", false);
            if (propId.StartsWith("PRP_COMBAT_TRAINING_")) return new GeoPropCategoryInfo("CBTRAIN", false);
            if (propId.StartsWith("PRP_RES_COMBAT_TRAINING_")) return new GeoPropCategoryInfo("CBTRAIN", false);
            if (propId.StartsWith("PRP_MINI_COMBAT_TRAINING_")) return new GeoPropCategoryInfo("CBTRAIN", true);
            if (propId.StartsWith("PRP_RES_MINI_COMBAT_TRAINING_")) return new GeoPropCategoryInfo("CBTRAIN", true);

            // --- Cardboard Boxes (CBOX) ---
            if (propId.StartsWith("PRP_CBOX_")) return new GeoPropCategoryInfo("CBOX", false);
            if (propId.StartsWith("PRP_MINI_CBOX_")) return new GeoPropCategoryInfo("CBOX", true);

            // --- Base Mission (BASE) ---
            if (propId.StartsWith("PRP_RES_01_MINI_BASE_HOME_") || propId.StartsWith("PRP_RES_02_MINI_BASE_HOME_") || propId.StartsWith("PRP_RES_03_MINI_BASE_HOME_") || propId.StartsWith("PRP_RES_04_MINI_BASE_HOME_"))
                return new GeoPropCategoryInfo("BASE", true);

            if (propId.StartsWith("PRP_MINI_BASE_HOME_")) return new GeoPropCategoryInfo("BASE", true);

            if (propId.StartsWith("PRP_RES_01_MINI_BASE_") || propId.StartsWith("PRP_RES_02_MINI_BASE_") || propId.StartsWith("PRP_RES_03_MINI_BASE_") || propId.StartsWith("PRP_RES_04_MINI_BASE_"))
                return new GeoPropCategoryInfo("BASE", true);

            if (propId.StartsWith("PRP_MINI_BASE_A") || propId.StartsWith("PRP_MINI_BASE_B")) return new GeoPropCategoryInfo("BASE", true);
            if (propId.StartsWith("PRP_MINI_BASE_")) return new GeoPropCategoryInfo("BASE", true);

            if (propId.StartsWith("PRP_RES_01_BASE_HOME_") || propId.StartsWith("PRP_RES_02_BASE_HOME_") || propId.StartsWith("PRP_RES_03_BASE_HOME_") || propId.StartsWith("PRP_RES_04_BASE_HOME_"))
                return new GeoPropCategoryInfo("BASE", false);
            if (propId.StartsWith("PRP_BASE_HOME_")) return new GeoPropCategoryInfo("BASE", false);
            if (propId.StartsWith("PRP_RES_01_BASE_") || propId.StartsWith("PRP_RES_02_BASE_") || propId.StartsWith("PRP_RES_03_BASE_") || propId.StartsWith("PRP_RES_04_BASE_"))
                return new GeoPropCategoryInfo("BASE", false);
            if (propId.StartsWith("PRP_BASE_A") || propId.StartsWith("PRP_BASE_B")) return new GeoPropCategoryInfo("BASE", false);
            if (propId.StartsWith("PRP_BASE_")) return new GeoPropCategoryInfo("BASE", false);

            return GeoPropCategoryInfo.Default;
        }

        private void RepackAllTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentStage == null)
            {
                MessageBox.Show("Please load a stage first.", "No Stage Loaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DldFile? cache = null;
            DldFile? cacheMips = null;

            using (var dldSelector = new DldSelector(CurrentStage))
            {
                if (dldSelector.ShowDialog(this) != DialogResult.OK)
                    return;

                if (string.IsNullOrEmpty(dldSelector.FilenameMain) || string.IsNullOrEmpty(dldSelector.FilenameMips))
                {
                    MessageBox.Show("You must select both a main and mips DLZ.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    cache = new DldFile(Path.Combine("stage", "_dlz", dldSelector.FilenameMain.Replace(".dlz", ".dld")));
                    cacheMips = new DldFile(Path.Combine("stage", "_dlz", dldSelector.FilenameMips.Replace(".dlz", ".dld")));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to load DLD files.");
                    MessageBox.Show($"Error loading DLD files: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            string? rootDdsPath = null;
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = "Select the root folder containing the texture subfolders (named after the .txn files)";
                if (fbd.ShowDialog(this) != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    return;

                rootDdsPath = fbd.SelectedPath;
            }

            Log.Information("Starting batch repack of all TXN files...");
            int repackedCount = 0;
            int skippedCount = 0;

            var txnFiles = CurrentStage.Files.Where(f => f.Type == StageFile.FileType.TXN).ToList();

            foreach (var txnFile in txnFiles)
            {
                string txnNameWithoutExt = Path.GetFileNameWithoutExtension(txnFile.Name);
                string ddsSubFolderPath = Path.Combine(rootDdsPath, txnNameWithoutExt);

                if (Directory.Exists(ddsSubFolderPath))
                {
                    Log.Information($"Repacking textures for '{txnFile.Name}' from '{ddsSubFolderPath}'");
                    try
                    {
                        Utils.RebuildTXN(ddsSubFolderPath, cache, cacheMips, txnFile.GetLocalPath());
                        repackedCount++;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Failed to repack {txnFile.Name}.");
                    }
                }
                else
                {
                    Log.Warning($"Skipping '{txnFile.Name}': Subfolder '{ddsSubFolderPath}' not found.");
                    skippedCount++;
                }
            }

            Log.Information("Batch repack finished.");
            MessageBox.Show($"Repacking complete.\n\nRepacked: {repackedCount} TXN file(s)\nSkipped: {skippedCount} TXN file(s)", "Finished", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DumpAllTxnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentStage == null)
            {
                MessageBox.Show("Please load a stage first.", "No Stage Loaded", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                return;

            string folderPath = fbd.SelectedPath;
            var textureData = new List<DldFile>();
            var dciFilesList = new List<DciFile>();

            foreach (var file in CurrentStage.Files)
            {
                if (file.Type == StageFile.FileType.DLZ)
                {
                    string dldPath = Path.Combine(file.GetUnpackedDir(), file.Name.Replace(".dlz", ".dld").Replace(".dec", ""));
                    textureData.Add(new DldFile(dldPath));
                }
                else if (file.Type == StageFile.FileType.DCI)
                {
                    dciFilesList.Add(new DciFile(file.GetLocalPath()));
                }
            }

            foreach (var file in CurrentStage.Files.Where(f => f.Type == StageFile.FileType.TXN))
            {
                var txnFile = new TxnFile(file.GetLocalPath());

                for (int txnIndex = 0; txnIndex < txnFile.Images.Count; txnIndex++)
                {
                    DldTexture? texture = null;
                    DldTexture? textureMips = null;
                    uint objectId = txnFile.ImageInfo[txnIndex].TriId;
                    int txnIndexUpdated = TxnEditor.GetIndexDldEntryDump(txnIndex, txnFile, dciFilesList);

                    for (int i = textureData.Count - 1; i >= 0; i--)
                    {
                        var dld = textureData[i];
                        texture ??= dld.FindTexture(objectId, txnIndexUpdated, DldPriority.Main);
                        textureMips ??= dld.FindTexture(objectId, txnIndexUpdated, DldPriority.Mipmaps);
                    }

                    string filename = DictionaryFile.GetHashString(txnFile.ImageInfo[txnIndex].TexId);
                    string fullDir = Path.Combine(folderPath, Path.GetFileNameWithoutExtension(txnFile.Path));
                    if (!Directory.Exists(fullDir))
                        Directory.CreateDirectory(fullDir);

                    if (texture == null && textureMips == null)
                    {
                        if (txnFile.HasEmbeddedTexture(txnIndex))
                        {
                            var ddsBytes = txnFile.ExtractEmbeddedDds(txnIndex);
                            File.WriteAllBytes(Path.Combine(fullDir, $"{filename}.dds"), ddsBytes);
                        }
                        else
                        {
                            Debug.WriteLine($"No texture found for {filename} in {txnFile.Path}");
                        }
                    }
                    else
                    {
                        if (texture == null && textureMips != null)
                            txnFile.CreateDdsFromIndex(Path.Combine(fullDir, $"{filename}.dds"), txnIndex, textureMips, null);
                        else
                            txnFile.CreateDdsFromIndex(Path.Combine(fullDir, $"{filename}.dds"), txnIndex, texture, textureMips);
                    }
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tabPageGeom.Show();
            SetEnabled(false);
            DictionaryFile.Load("bin/dictionary.txt", "bin/dictionary-aliases.txt");
            SetupContextMenus();

            Scene.MeshSelected += mesh =>
            {
                if (mesh != null && TreeNodeLookup.TryGetValue(mesh.ID, out var node))
                {
                    treeViewGeom.SelectedNode = node;
                    treeViewGeom.Focus();
                }
            };

            Scene.DragSelectDone += Scene_DragSelectDone;

            Log.Information("Initialized");
        }

        private void Scene_DragSelectDone(List<Mesh>? obj)
        {
            if (obj == null || obj.Count == 0)
                return;

            if (obj.Count == 1)
            {
                MeshGeomPropLookup.TryGetValue(obj[0], out var prop);

                if (prop == null)
                {
                    return;
                }

                using (var propEditor = new PropEditor(Scene, obj[0], prop))
                {
                    propEditor.ShowDialog();
                }

                return;
            }

            var props = new List<GeomProp>();

            foreach (var mesh in obj)
            {
                MeshGeomPropLookup.TryGetValue(mesh, out var prop);

                if (prop != null)
                {
                    props.Add(prop);
                }
            }

            glControl.Invalidate();

            using (var propEditor = new PropEditorMulti(props, GeomPropMeshLookup))
            {
                propEditor.ShowDialog();
                Scene.ClearSelection();
            }
        }

        private void SetupContextMenus()
        {
            // files
            var menuItems = new List<ToolStripMenuItem>();

            MenuItemFilesEdit.Text = "Edit";
            menuItems.Add(MenuItemFilesEdit);

            MenuItemFilesOpen.Text = "Open in Explorer";
            menuItems.Add(MenuItemFilesOpen);

            MenuItemFilesRebuild.Text = "Repack Textures";
            menuItems.Add(MenuItemFilesRebuild);

            ContextMenuFiles.ItemClicked += ContextMenuFiles_ItemClicked;
            ContextMenuFiles.Items.AddRange(menuItems.ToArray());

            // geom prop
            menuItems = new List<ToolStripMenuItem>();

            MenuItemGeomPropEdit.Text = "Edit";
            menuItems.Add(MenuItemGeomPropEdit);

            ContextMenuGeomProp.ItemClicked += ContextMenuGeomProp_ItemClicked;
            ContextMenuGeomProp.Items.AddRange(menuItems.ToArray());

            // geom mesh
            menuItems = new List<ToolStripMenuItem>();

            MenuItemGeomMeshEdit.Text = "Edit";
            menuItems.Add(MenuItemGeomMeshEdit);

            ContextMenuGeomMesh.ItemClicked += ContextMenuGeomMesh_ItemClicked;
            ContextMenuGeomMesh.Items.AddRange(menuItems.ToArray());
        }

        private void ContextMenuGeomMesh_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (Geom == null || treeViewGeom.SelectedNode == null || CurrentStage == null)
                return;

            Mesh? mesh = Mesh.MeshList.Find(m => m.ID == treeViewGeom.SelectedNode.Text);

            if (mesh == null)
                return;

            if (e.ClickedItem == MenuItemGeomMeshEdit)
            {
                GeoBlock? block;

                if (!GeomMesh.BlockLookup.TryGetValue(mesh, out block) || block == null)
                    return;

                using (var geomEditor = new GeomEditor(Geom, block, mesh, Scene))
                {
                    geomEditor.ShowDialog();
                }
            }
        }

        private void ContextMenuGeomProp_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (treeViewGeom.SelectedNode == null || CurrentStage == null)
                return;

            GeomProp? prop;

            if (!GeomPropLookup.TryGetValue(treeViewGeom.SelectedNode, out prop) || prop == null)
                return;

            Mesh? propMesh;

            if (!GeomPropMeshLookup.TryGetValue(prop, out propMesh) || propMesh == null)
                return;

            if (e.ClickedItem == MenuItemGeomPropEdit)
            {
                using (var propEditor = new PropEditor(Scene, propMesh, prop))
                {
                    propEditor.ShowDialog();
                }
            }
        }

        private void ContextMenuFiles_ItemClicked(object? sender, ToolStripItemClickedEventArgs e)
        {
            if (treeViewFiles.SelectedNode == null || CurrentStage == null)
                return;

            StageFile stageFile = StageFileLookup[treeViewFiles.SelectedNode];

            if (stageFile == null)
                return;

            if (e.ClickedItem == MenuItemFilesOpen)
            {
                switch (stageFile.Type)
                {
                    case StageFile.FileType.QAR:
                    case StageFile.FileType.DAR:
                        Utils.ExplorerOpenDirectory(stageFile.GetUnpackedDir());
                        break;
                    default:
                        Utils.ExplorerSelectFile(stageFile.GetLocalPath());
                        break;
                }
            }
            else if (e.ClickedItem == MenuItemFilesEdit)
            {
                switch (stageFile.Type)
                {
                    case StageFile.FileType.DCI:
                        using (var dciEditor = new DciEditor(stageFile, CurrentStage))
                        {
                            dciEditor.ShowDialog();
                        }
                        break;
                    case StageFile.FileType.DLZ:
                        using (var dldEditor = new DldEditor(stageFile, CurrentStage))
                        {
                            dldEditor.ShowDialog();
                        }
                        break;
                    case StageFile.FileType.TXN:
                        using (var txnEditor = new TxnEditor(stageFile, CurrentStage))
                        {
                            txnEditor.ShowDialog();
                        }
                        break;
                    case StageFile.FileType.CNF:
                    case StageFile.FileType.NNI:
                        using (var txtEditor = new TextEditor(stageFile, false))
                        {
                            txtEditor.ShowDialog();
                        }
                        break;
                    default:
                        MessageBox.Show("Unsupported file type", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
            else if (e.ClickedItem == MenuItemFilesRebuild)
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    DldFile? cache = null;
                    DldFile? cacheMips = null;

                    using (var dldSelector = new DldSelector(CurrentStage))
                    {
                        dldSelector.ShowDialog();

                        if (dldSelector.FilenameMain == "" || dldSelector.FilenameMips == "")
                        {
                            MessageBox.Show("You must select a DLZ.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }

                        cache = new DldFile($"stage\\_dlz\\{dldSelector.FilenameMain.Replace(".dlz", ".dld")}");
                        cacheMips = new DldFile($"stage\\_dlz\\{dldSelector.FilenameMips.Replace(".dlz", ".dld")}");
                    }

                    DialogResult result = fbd.ShowDialog();

                    if (result != DialogResult.OK || string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    {
                        return;
                    }

                    Utils.RebuildTXN(fbd.SelectedPath, cache, cacheMips, stageFile.GetLocalPath());
                }
            }
        }

        private void SetEnabled(bool flag)
        {
            if (flag)
            {
                tabControl.Enabled = true;
                btnSave.Enabled = true;
                labelStatus.Text = "Ready";
            }
            else
            {
                tabControl.Enabled = false;
                btnSave.Enabled = false;
            }
        }

        private void Reset()
        {
            if (Geom != null)
            {
                Geom.CloseStream();
                Geom.Clear();
            }

            MeshGroups.Clear();
            MeshRefs.Clear();
            MeshProps.Clear();
            GeomPropMeshLookup.Clear();
            MeshGeomPropLookup.Clear();
            CurrentStage = null;
            Geom = null;
            Scene.Children.Clear();
            treeViewFiles.Nodes.Clear();
            treeViewGeom.Nodes.Clear();
            GeomMesh.BlockLookup.Clear();
            GeomMesh.MeshLookup.Clear();
            PropEditor.GeomPropOriginal.Clear();
            Mesh.ResetID();

            foreach (var mesh in Mesh.MeshList)
            {
                mesh.Delete();
            }

            Mesh.MeshList.Clear();

            if (Directory.Exists("stage"))
            {
                Directory.Delete("stage", true);
            }

            Directory.CreateDirectory("stage");
        }

        private async Task SetupGeom(string path)
        {
            try
            {
                var copyPath = $"{path}.edit";

                if (File.Exists(copyPath))
                    File.Delete(copyPath);

                File.Copy(path, copyPath);

                await Task.Run(() => Geom = new GeomFile(copyPath));

                if (Geom != null)
                {
                    MeshGroups = await Task.Run(() => GeomMesh.GetGeomGroupMeshes(Geom));
                    MeshRefs = await Task.Run(() => GeomMesh.GetGeomRefMeshes(Geom));
                    MeshBoundaries = await Task.Run(() => GeomMesh.GetGeomBoundaryMeshes(Geom));

                    var low = new Vector4();
                    var high = new Vector4();

                    Geom.GetWorldBoundary(ref low, ref high);


                    Scene.UpdateGridProperties(low, high);
                }

                await Task.Run(() =>
                {
                    MeshGroups.ForEach(mesh => Scene.Children.Add(mesh));

                    MeshRefs.ForEach(mesh =>
                    {
                        mesh.Visible = false;
                        Scene.Children.Add(mesh);
                    });

                    MeshBoundaries.ForEach(mesh =>
                    {
                        mesh.Visible = false;
                        Scene.Children.Add(mesh);
                    });
                });

                await Task.Run(() => GenerateGeomPropMeshes());
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Geom Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetPropModel(string id)
        {
            if (id.Contains("PRP_"))
            {
                if (id.Contains("_GOAL"))
                {
                    return Resources.ModelTerminal;
                }
                else if (id.Contains("_TGT_00"))
                {
                    return Resources.ModelGako;
                }
                else if (id.Contains("_TGT_01"))
                {
                    return Resources.ModelKerotan;
                }
                else if (id.Contains("_CBOX"))
                {
                    return Resources.ModelCBOX;
                }
                else if (id.Contains("_DOLL_"))
                {
                    return Resources.ModelDoll;
                }
            }

            return Resources.ModelCube;
        }

        private void GenerateGeomPropMeshes()
        {
            if (Geom == null)
                return;

            foreach (var prop in Geom.GeomProps)
            {
                if (prop.X == 0 && prop.Y == 0 && prop.Z == 0 && prop.W == 0)
                    continue;

                string id = DictionaryFile.GetHashString(prop.Hash);
                Mesh mesh = Mesh.LoadFromPLYBuffer(GetPropModel(id), new Vector3d(prop.X, prop.Z, prop.Y));
                var color = Color.FromArgb(255, 100, 140, 100);

                mesh.ColorStatic = color;
                mesh.SetColor(color);
                mesh.UseVertexColor = true;
                mesh.Visible = false;
                mesh.ID = id;
                mesh.DragSelectable = true;
                MeshProps.Add(mesh);
                Scene.Children.Add(mesh);
                GeomPropMeshLookup[prop] = mesh;
                MeshGeomPropLookup[mesh] = prop;
            }
        }

        private void PopulateGeomTreeView(string text)
        {
            if (Geom == null)
                return;

            _suspendAfterCheck = true;
            treeViewGeom.Enabled = false;
            treeViewGeom.BeginUpdate();

            string filterTextLower = text.ToLower();

            // Reset lookups
            GeomPropLookup.Clear();
            TreeNodeLookup.Clear();

            // ----------------------
            // 1) MeshGroups
            // ----------------------
            TreeNodeGeomMeshes.Nodes.Clear();
            foreach (var mesh in MeshGroups.OrderBy(x => x.ID))
            {
                if (!mesh.ID.ToLower().Contains(filterTextLower))
                    continue;

                var node = TreeNodeGeomMeshes.Nodes.Add(mesh.ID);
                node.Name = mesh.ID;
                node.Checked = mesh.Visible;   // ✅ keep actual visibility
                node.Tag = mesh;
                TreeNodeLookup[mesh.ID] = node;
            }

            // ----------------------
            // 2) MeshRefs
            // ----------------------
            TreeNodeGeomRefs.Nodes.Clear();
            foreach (var mesh in MeshRefs.OrderBy(x => x.ID))
            {
                if (!mesh.ID.ToLower().Contains(filterTextLower))
                    continue;

                var node = TreeNodeGeomRefs.Nodes.Add(mesh.ID);
                node.Name = mesh.ID;
                node.Checked = mesh.Visible;   // ✅ keep actual visibility
                node.Tag = mesh;
                TreeNodeLookup[mesh.ID] = node;
            }

            // ----------------------
            // 3) MeshBoundaries
            // ----------------------
            TreeNodeGeomBoundaries.Nodes.Clear();
            foreach (var mesh in MeshBoundaries.OrderBy(x => x.ID))
            {
                if (!mesh.ID.ToLower().Contains(filterTextLower))
                    continue;

                var node = TreeNodeGeomBoundaries.Nodes.Add(mesh.ID);
                node.Name = mesh.ID;
                node.Checked = mesh.Visible;   // ✅ keep actual visibility
                node.Tag = mesh;
                TreeNodeLookup[mesh.ID] = node;
            }

            // ----------------------
            // 4) GeomProps with categorization (from first version)
            // ----------------------
            TreeNodeGeomProps.Nodes.Clear();

            var categorizedRuleProps = new Dictionary<string, Dictionary<bool, List<TreeNode>>>();
            var otherPropsList = new List<TreeNode>();

            var allSortedGeomProps = Geom.GeomProps
                .Select(p => new { Prop = p, MeshID = DictionaryFile.GetHashString(p.Hash) })
                .OrderBy(pInfo => pInfo.MeshID)
                .ToList();

            foreach (var propInfo in allSortedGeomProps)
            {
                GeomProp prop = propInfo.Prop;
                string propStringId = propInfo.MeshID;

                if (!GeomPropMeshLookup.TryGetValue(prop, out Mesh? associatedMesh))
                    continue;

                if (!propStringId.ToLower().Contains(filterTextLower))
                    continue;

                TreeNode propEntryNode = new TreeNode(propStringId);
                propEntryNode.Name = propStringId;
                propEntryNode.Tag = associatedMesh;
                propEntryNode.Checked = associatedMesh.Visible;   // ✅ keep actual visibility

                TreeNodeLookup[propStringId] = propEntryNode;
                GeomPropLookup[propEntryNode] = prop;

                GeoPropCategoryInfo catInfo = GetPropCategoryInfo(propStringId);

                if (catInfo.CategoryName == "Other Props")
                {
                    otherPropsList.Add(propEntryNode);
                }
                else
                {
                    if (!categorizedRuleProps.TryGetValue(catInfo.CategoryName, out var subCategories))
                    {
                        subCategories = new Dictionary<bool, List<TreeNode>>();
                        categorizedRuleProps[catInfo.CategoryName] = subCategories;
                    }
                    if (!subCategories.TryGetValue(catInfo.IsMini, out var specificPropList))
                    {
                        specificPropList = new List<TreeNode>();
                        subCategories[catInfo.IsMini] = specificPropList;
                    }
                    specificPropList.Add(propEntryNode);
                }
            }

            var sortedRuleCategoryNames = categorizedRuleProps.Keys.OrderBy(name => name).ToList();

            foreach (string categoryName in sortedRuleCategoryNames)
            {
                var subCategories = categorizedRuleProps[categoryName];
                TreeNode ruleParentNode = new TreeNode(categoryName + " Props");
                TreeNodeGeomProps.Nodes.Add(ruleParentNode);

                if (subCategories.TryGetValue(false, out var normalPropsList) && normalPropsList.Count > 0)
                {
                    TreeNode normalCategoryNode = new TreeNode("Main");
                    ruleParentNode.Nodes.Add(normalCategoryNode);

                    normalCategoryNode.Nodes.AddRange(normalPropsList.ToArray());
                }

                if (subCategories.TryGetValue(true, out var miniPropsList) && miniPropsList.Count > 0)
                {
                    TreeNode miniCategoryNode = new TreeNode("Mini");
                    ruleParentNode.Nodes.Add(miniCategoryNode);
                    miniCategoryNode.Nodes.AddRange(miniPropsList.ToArray());
                }
            }

            if (otherPropsList.Count > 0)
            {
                TreeNode otherPropsParentNode = new TreeNode("Other Props");
                TreeNodeGeomProps.Nodes.Add(otherPropsParentNode);
                otherPropsParentNode.Nodes.AddRange(otherPropsList.ToArray());
            }

            // Ensure "Other Props" always last
            var topLevelPropNodes = TreeNodeGeomProps.Nodes.Cast<TreeNode>().ToList();
            TreeNode? otherNodeGlobal = topLevelPropNodes.FirstOrDefault(n => n.Text == "Other Props");
            List<TreeNode> sortedTopLevelPropNodes = new List<TreeNode>();

            if (otherNodeGlobal != null)
                topLevelPropNodes.Remove(otherNodeGlobal);

            sortedTopLevelPropNodes.AddRange(topLevelPropNodes.OrderBy(n => n.Text));

            if (otherNodeGlobal != null)
                sortedTopLevelPropNodes.Add(otherNodeGlobal);

            if (!TreeNodeGeomProps.Nodes.Cast<TreeNode>().SequenceEqual(sortedTopLevelPropNodes))
            {
                TreeNodeGeomProps.Nodes.Clear();
                TreeNodeGeomProps.Nodes.AddRange(sortedTopLevelPropNodes.ToArray());
            }

            treeViewGeom.EndUpdate();
            treeViewGeom.Enabled = true;
            _suspendAfterCheck = false;
        }



        private async void PromptStageLoad(Stage.GameType game)
        {
            SetEnabled(false);

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Reset();
                    CurrentStage = new Stage(fbd.SelectedPath, game);

                    if (game == Stage.GameType.MGO2)
                    {
                        BinaryWriterEx.DefaultBigEndian = true;
                        BinaryReaderEx.DefaultBigEndian = true;
                        labelStatus.Text = "Decrypting...";
                        await CurrentStage.Decrypt();
                    }
                    else
                    {
                        BinaryWriterEx.DefaultBigEndian = game == Stage.GameType.MGS4;
                        BinaryReaderEx.DefaultBigEndian = game == Stage.GameType.MGS4;
                        labelStatus.Text = "Copying...";
                        CurrentStage.Copy("stage");
                    }

                    labelStatus.Text = "Unpacking...";
                    await CurrentStage.Unpack();

                    foreach (var file in CurrentStage.Files)
                    {
                        string ext = Path.GetExtension(file.Name);

                        if (ext == ".dec" || ext == ".enc")
                            continue;

                        FileInfo fi = new FileInfo(file.Name);
                        TreeNode? tds = null;

                        if (file.Archive != null)
                        {
                            var parent = treeViewFiles.Nodes.Find(file.Archive.Name, false);

                            if (parent.Length > 0)
                            {
                                tds = parent[0].Nodes.Add(fi.Name);
                                tds.Name = fi.Name;
                                StageFileLookup[tds] = file;
                            }
                        }
                        else
                        {
                            tds = treeViewFiles.Nodes.Add(fi.Name);
                            tds.Name = fi.Name;
                            StageFileLookup[tds] = file;
                        }

                        if (tds != null)
                        {
                            tds.Tag = fi.FullName;
                            tds.StateImageIndex = 0;

                            if (ext == ".qar" || ext == ".dar")
                                tds.StateImageIndex = 1;
                        }
                    }

                    TreeNodeGeomMeshes = treeViewGeom.Nodes.Add("Meshes");
                    TreeNodeGeomProps = treeViewGeom.Nodes.Add("Props");
                    TreeNodeGeomRefs = treeViewGeom.Nodes.Add("References");
                    TreeNodeGeomBoundaries = treeViewGeom.Nodes.Add("Boundaries");

                    treeViewGeom.CheckBoxes = true;
                    TreeNodeGeomMeshes.Checked = true;

                    if (CurrentStage.Geom != null)
                    {
                        labelStatus.Text = "Loading geom...";

                        await SetupGeom($"stage/{CurrentStage.Geom.Name}.dec");

                        PopulateGeomTreeView("");
                    }

                    var centerStage = Mesh.FromID("PRP_STAGE_CENTER");

                    if (centerStage != null && Scene != null)
                    {
                        Scene.Camera.Position = centerStage.Center;
                    }

                    SetEnabled(true);
                }
                else if (CurrentStage != null)
                {
                    SetEnabled(true);
                }
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            if (CurrentStage == null) return;

            SetEnabled(false);

            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    CurrentStage.Key = Directory.GetParent(fbd.SelectedPath)?.Name + "/" + new DirectoryInfo(fbd.SelectedPath).Name;

                    labelStatus.Text = "Packing...";
                    await CurrentStage.Pack();

                    if (Geom != null && CurrentStage.Geom != null)
                    {
                        labelStatus.Text = "Saving geom...";
                        Geom.Save(CurrentStage.Geom.GetLocalPath());
                    }

                    if (CurrentStage.Game == Stage.GameType.MGO2)
                    {
                        labelStatus.Text = "Encrypting...";
                        await CurrentStage.Encrypt(fbd.SelectedPath);
                    }
                    else
                    {
                        labelStatus.Text = "Copying...";
                        CurrentStage.CopyOut(fbd.SelectedPath);
                    }

                    var files = Directory.GetFiles(fbd.SelectedPath);

                    foreach (var file in files)
                    {
                        string newName = file.Replace(".dec", "").Replace(".enc", "");

                        if (newName != file && File.Exists(newName))
                            File.Delete(newName);

                        File.Move(file, newName);
                    }
                }
            }

            SetEnabled(true);
        }

        private void timerRefresh_Tick(object sender, EventArgs e)
        {
            if (Scene != null && tabControl.SelectedTab == tabPageGeom)
            {
                labelCamPos.Text = $"{(int)Scene.Camera.Position.X}, {(int)Scene.Camera.Position.Y}, {(int)Scene.Camera.Position.Z}";

                if (Scene.SelectedDrawable != null && glControl.Focused)
                {
                    var meshId = (Scene.SelectedDrawable as Mesh)?.ID;

                    labelCamPos.Text += $" | {meshId}";
                }
            }
            else
            {
                labelCamPos.Text = "";
            }
        }

        private void treeViewGeom_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }

        private bool CalculatePropVisibility(TreeNode propLeafNode)
        {
            if (propLeafNode == null || !(propLeafNode.Tag is Mesh)) return false;


            TreeNode? root = GetRootCategoryNode(propLeafNode);
            if (root != TreeNodeGeomProps) return false;

            TreeNode? current = propLeafNode;
            while (current != null)
            {
                if (!current.Checked)
                {
                    return false;
                }
                if (current == TreeNodeGeomProps)
                {
                    return true;
                }
                current = current.Parent;
            }

            return false;
        }
        private void PropagateCheckStateToChildren(TreeNode parentNode, bool isChecked)
        {
            foreach (TreeNode childNode in parentNode.Nodes)
            {
                if (childNode.Checked != isChecked)
                {
                    childNode.Checked = isChecked;
                }
                if (childNode.Nodes.Count > 0)
                {
                    PropagateCheckStateToChildren(childNode, isChecked);
                }
            }
        }

        private void UpdateAllPropMeshVisibilities()
        {
            if (TreeNodeGeomProps == null || Geom == null || Geom.GeomProps == null) return;

            Queue<TreeNode> nodesToVisit = new Queue<TreeNode>();
            nodesToVisit.Enqueue(TreeNodeGeomProps);

            while (nodesToVisit.Count > 0)
            {
                TreeNode currentNode = nodesToVisit.Dequeue();
                foreach (TreeNode childNode in currentNode.Nodes)
                {
                    nodesToVisit.Enqueue(childNode);
                }

                // If the current node is a leaf prop node
                if (currentNode.Tag is Mesh propMesh && GetRootCategoryNode(currentNode) == TreeNodeGeomProps)
                {
                    propMesh.Visible = CalculatePropVisibility(currentNode);
                }
            }
        }

        private void UpdateNonPropMeshVisibilities(TreeNode? topLevelNode)
        {
            if (topLevelNode == null) return;

            Queue<TreeNode> nodesToVisit = new Queue<TreeNode>();
            nodesToVisit.Enqueue(topLevelNode);

            while (nodesToVisit.Count > 0)
            {
                TreeNode currentNode = nodesToVisit.Dequeue();

                if (currentNode.Tag is Mesh mesh)
                {
                    mesh.Visible = currentNode.Checked;
                }

                foreach (TreeNode childNode in currentNode.Nodes)
                {
                    nodesToVisit.Enqueue(childNode);
                }
            }
        }

        private void UpdateParentVisualCheckState(TreeNode? parentNode)
        {
            if (parentNode == null) return;

            if (parentNode.Nodes.Count > 0)
            {
                bool anyChildChecked = parentNode.Nodes.Cast<TreeNode>().Any(n => n.Checked);
                if (parentNode.Checked != anyChildChecked)
                {
                    parentNode.Checked = anyChildChecked;
                }
            }

            if (parentNode.Parent != null)
            {
                UpdateParentVisualCheckState(parentNode.Parent);
            }
        }

        private void treeViewGeom_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == null)
                return;

            if (e.Node.Tag is Mesh mesh)
            {

                TreeNode? rootCategory = GetRootCategoryNode(e.Node);

                if (rootCategory != null)
                {
                    bool cameraMoved = false;
                    if (rootCategory == TreeNodeGeomProps)
                    {
                        Scene.Camera.Position = mesh.AABB.TransformedCenter;
                        cameraMoved = true;
                    }
                    else if (rootCategory == TreeNodeGeomMeshes || rootCategory == TreeNodeGeomRefs)
                    {
                        if (mesh.Vertices != null && mesh.Vertices.Count() > 0)
                        {
                            Scene.Camera.Position = mesh.Vertices[0];
                        }
                        else
                        {
                            Scene.Camera.Position = mesh.AABB.TransformedCenter;
                        }
                        cameraMoved = true;
                    }
                    else if (rootCategory == TreeNodeGeomBoundaries)
                    {
                        Scene.Camera.Position = mesh.AABB.TransformedCenter;
                        cameraMoved = true;
                    }

                    Scene.SelectMesh(mesh);

                    if (cameraMoved)
                    {
                        Scene.Render();
                    }
                    else
                    {
                        Scene.Render();
                    }
                }
            }
        }

        private bool _isUpdatingTreeChecks = false;

private void treeViewGeom_BeforeCheck(object sender, TreeViewCancelEventArgs e)
{
    if (_suspendAfterCheck) return;

    if (e.Node == TreeNodeGeomMeshes ||
        e.Node == TreeNodeGeomRefs ||
        e.Node == TreeNodeGeomProps ||
        e.Node == TreeNodeGeomBoundaries)
    {
        e.Cancel = true;
        bool newState = !e.Node.Checked;

        _suspendAfterCheck = true;


        e.Node.Checked = newState;

        foreach (TreeNode child in e.Node.Nodes)
        {
            child.Checked = newState;
            if (child.Tag is Mesh m)
                m.Visible = newState;
        }

        _suspendAfterCheck = false;

        treeViewGeom.SelectedNode = null;
        Scene.Render();
    }
}


        private void treeViewGeom_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_suspendAfterCheck || e.Action == TreeViewAction.Unknown || e.Node == null)
                return;

            if (_isUpdatingTreeChecks)
                return;

            try
            {
                _isUpdatingTreeChecks = true;

                if (e.Node.Tag is Mesh mesh)
                {
                    mesh.Visible = e.Node.Checked;
                }

                if (e.Node.Nodes.Count > 0)
                {
                    PropagateCheckStateToChildren(e.Node, e.Node.Checked);
                }

                if (e.Node.Parent != null)
                {
                    UpdateParentVisualCheckState(e.Node.Parent);
                }

                UpdateNonPropMeshVisibilities(TreeNodeGeomMeshes);
                UpdateNonPropMeshVisibilities(TreeNodeGeomRefs);
                UpdateNonPropMeshVisibilities(TreeNodeGeomBoundaries);
                UpdateAllPropMeshVisibilities();
            }
            finally
            {
                _isUpdatingTreeChecks = false;
            }

            Scene.Render();
        }


        private void UpdateParentNodeCheckState(TreeNode parentNode)
        {
            if (parentNode == null || _isUpdatingTreeChecks)
                return;

            try
            {
                _isUpdatingTreeChecks = true;

                if (parentNode.Nodes.Count == 0)
                {
                }
                else
                {
                    bool allChildrenChecked = true;
                    bool anyChildChecked = false;

                    foreach (TreeNode childNode in parentNode.Nodes)
                    {
                        if (childNode.Checked)
                        {
                            anyChildChecked = true;
                        }
                        else
                        {
                            allChildrenChecked = false;
                        }
                    }

                    bool newParentState;
                    if (anyChildChecked && !allChildrenChecked)
                    {
                        newParentState = true;
                    }
                    else
                    {
                        newParentState = allChildrenChecked;
                    }

                    if (parentNode.Checked != newParentState)
                    {
                        parentNode.Checked = newParentState;
                    }
                }
            }
            finally
            {
                _isUpdatingTreeChecks = false;
            }
        }

        private TreeNode? GetRootCategoryNode(TreeNode? node)
        {
            if (node == null) return null;
            TreeNode currentNode = node;
            while (currentNode.Parent != null)
            {
                currentNode = currentNode.Parent;
            }
            if (currentNode == TreeNodeGeomMeshes ||
                currentNode == TreeNodeGeomProps ||
                currentNode == TreeNodeGeomRefs ||
                currentNode == TreeNodeGeomBoundaries)
            {
                return currentNode;
            }
            return null;
        }

        private void tbSpawnsFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PopulateGeomTreeView(tbSpawnsFilter.Text);
            }
        }

        private void cbWireframe_CheckStateChanged(object sender, EventArgs e)
        {
            GL.Disable(EnableCap.PolygonSmooth);

            switch (cbWireframe.CheckState)
            {
                case CheckState.Unchecked:
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    break;
                case CheckState.Checked:
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                    break;
                case CheckState.Indeterminate:
                    GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    GL.Enable(EnableCap.PolygonSmooth);
                    break;
                default:
                    break;
            }

            Scene.Render();
        }

        private void btnExportMesh_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "PLY files (*.ply)|*.ply";
                saveFileDialog1.RestoreDirectory = true;

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    List<Mesh> meshes = new List<Mesh>();

                    foreach (var child in Scene.Children)
                    {
                        if (!child.Visible)
                            continue;

                        meshes.Add(child as Mesh);
                    }

                    Mesh mesh = Mesh.CombineMeshes(meshes);
                    mesh.SaveMesh(saveFileDialog1.FileName);
                }
            }
        }

        private async void encryptFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var path = openFileDialog.FileName;
                    await Utils.EncryptFileAsync(path, Utils.GetPathKey(path));
                }
            }
        }

        private async void decryptFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var path = openFileDialog.FileName;
                    await Utils.DecryptFileAsync(path, Utils.GetPathKey(path));
                }
            }
        }

        private void mergeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var baseFilePath = string.Empty;
            var mergeFilePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    baseFilePath = openFileDialog.FileName;
                }
            }

            if (baseFilePath == string.Empty)
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    mergeFilePath = openFileDialog.FileName;
                }
            }

            try
            {
                var baseGeom = new GeomFile(baseFilePath);
                var mergeGeom = new GeomFile(mergeFilePath);
                baseGeom.Merge(mergeGeom);
                mergeGeom.CloseStream();

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        baseGeom.Save(saveFileDialog.FileName);
                    }
                }

                baseGeom.CloseStream();
            }
            catch (Exception exception)
            {
                // leaks if failed
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void mergeVLMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var baseFilePath = string.Empty;
            var mergeFilePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "vlm files (*.vlm)|*.vlm|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    baseFilePath = openFileDialog.FileName;
                }
            }

            if (baseFilePath == string.Empty)
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "vlm files (*.vlm)|*.vlm|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    mergeFilePath = openFileDialog.FileName;
                }
            }

            try
            {
                var vlmBase = new VlmFile(baseFilePath);
                var vlmMerge = new VlmFile(mergeFilePath);
                vlmBase.Merge(vlmMerge);

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "vlm files (*.vlm)|*.vlm|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        vlmBase.Save(saveFileDialog.FileName);
                    }
                }
            }
            catch (Exception exception)
            {
                // leaks if failed
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            var baseFilePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    baseFilePath = openFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var baseGeom = new GeomFile(baseFilePath, false);
                    baseGeom.Save(saveFileDialog.FileName, true);
                    baseGeom.CloseStream();
                }
            }
        }

        private void stringHashUtilityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new StringHashEditor().ShowDialog();
        }

        private void treeViewFiles_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Select the clicked node
                treeViewFiles.SelectedNode = treeViewFiles.GetNodeAt(e.X, e.Y);

                if (treeViewFiles.SelectedNode != null)
                {
                    string[] canEdit = new string[] { ".nni", ".cnf", ".txn", ".dlz", ".dci" };
                    string filename = treeViewFiles.SelectedNode.Text;
                    MenuItemFilesEdit.Enabled = canEdit.Contains(Path.GetExtension(filename));

                    ContextMenuFiles.Show(treeViewFiles, e.Location);

                    MenuItemFilesRebuild.Visible = filename.Contains(".txn");
                }
            }
        }

        private void treeViewGeom_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeNode? clickedNode = treeViewGeom.GetNodeAt(e.X, e.Y);
                if (clickedNode != null)
                {
                    treeViewGeom.SelectedNode = clickedNode;

                    TreeNode? rootCategory = null;

                    if (clickedNode.Tag is Mesh)
                    {
                        rootCategory = GetRootCategoryNode(clickedNode);
                    }

                    if (rootCategory == TreeNodeGeomProps)
                    {
                        ContextMenuGeomProp.Show(treeViewGeom, e.Location);
                    }
                    else if (rootCategory == TreeNodeGeomMeshes || rootCategory == TreeNodeGeomRefs)
                    {
                        ContextMenuGeomMesh.Show(treeViewGeom, e.Location);
                    }
                }
            }
        }


        private void mGO2StageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PromptStageLoad(Stage.GameType.MGO2);
        }

        private void mGS4StageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PromptStageLoad(Stage.GameType.MGS4);
        }

        private void mGAStageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PromptStageLoad(Stage.GameType.MGA);
        }

        private void mergeReferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var baseFilePath = string.Empty;
            var mergeFilePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    baseFilePath = openFileDialog.FileName;
                }
            }

            if (baseFilePath == string.Empty)
                return;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    mergeFilePath = openFileDialog.FileName;
                }
            }

            try
            {
                var baseGeom = new GeomFile(baseFilePath);
                var mergeGeom = new GeomFile(mergeFilePath);
                //baseGeom.CopySingleRef(mergeGeom, 0x874c5);
                baseGeom.MergeReferences(mergeGeom);
                mergeGeom.CloseStream();

                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "geom files (*.geom)|*.geom|All files (*.*)|*.*";
                    saveFileDialog.FilterIndex = 1;
                    saveFileDialog.RestoreDirectory = true;

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        baseGeom.Save(saveFileDialog.FileName);
                    }
                }

                baseGeom.CloseStream();
            }
            catch (Exception exception)
            {
                // leaks if failed
                MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void AppendLog(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendLog), new object[] { value });
                return;
            }
            tbLog.Text += value;
            tbLog.SelectionStart = tbLog.TextLength;
            tbLog.ScrollToCaret();
        }

        private void LoggerSink_NewLogHandler(object? sender, EventArgs e)
        {
            var log = ((LogEventArgs)e).Log;
            using var writer = new StringWriter();
            LoggerSink.Formatter.Format(log, writer);
            var message = writer.ToString();
            AppendLog(message);
        }

        private void cbGrid_CheckedChanged(object sender, EventArgs e)
        {
            if (Scene.CurrentScene != null)
            {
                Scene.CurrentScene.GridEnabled = cbGrid.Checked;
                Scene.CurrentScene.Render();
            }
        }
    }
}