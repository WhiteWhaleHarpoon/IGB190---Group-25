using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

public class AbilityEditor : EditorWindow
{
    private Ability ability = null;
    private LogicEngineEditor engineEditor;
    private const string resourceSubFolder = "Abilities";
    private const string templatesResourceFolder = "Templates";
    private const string fullAbilitiesFolderPath = "Assets/Resources/Abilities";
    private const float abilityListPanelWidth = 200;
    private const float abilityDetailsPanelWidth = 230;
    private Color panelColor = new Color(0.3f, 0.3f, 0.3f);
    private Color selectedColor = new Color(.17f, .36f, .53f);
    private Color unselectedColor = new Color(0.15f, 0.15f, 0.15f);
    private Vector2 abilityListScrollPosition;

    public void SetSelectedAbility (Ability ability)
    {
        this.ability = ability;
        engineEditor = new LogicEngineEditor(this, ability.engine, ability);
    }

    /// <summary>
    /// Draw the list of all abilities in the game.
    /// </summary>
    private void DrawAbilityList (Rect panel)
    {
        float posX = panel.x;
        float posY = panel.y;
        float headerHeight = 25;
        float itemHeight = 30;
        float spacer = 5;
        float itemPadding = 1;
        float width = panel.width;
        
        // Draw the background panel color.
        EditorGUI.DrawRect(panel, panelColor);

        // Draw the header.
        Rect area = new Rect(posX, posY, width, headerHeight);
        EditorGUI.DrawRect(area, new Color(0.1f, 0.1f, 0.1f, 1));
        EditorGUI.LabelField(area, "  Abilities ", LogicEngineEditor.windowStyle_HeaderText);
        if (GUI.Button(new Rect(area.xMax - headerHeight, area.y - 1, headerHeight, 
            headerHeight), "+", LogicEngineEditor.windowStyle_AddButton))
        {
            CreateNewAbility();
        }
        posY += headerHeight;

        // Cache the ability list.
        Ability[] abilities = Resources.LoadAll<Ability>(resourceSubFolder);

        // Set up ability list scrolling.
        Rect scrollRect = new Rect(panel);
        scrollRect.y += headerHeight;
        scrollRect.height -= headerHeight;
        Rect requiredSize = new Rect(scrollRect);
        requiredSize.height = (itemHeight + itemPadding) * abilities.Length;
        abilityListScrollPosition = GUI.BeginScrollView(scrollRect, abilityListScrollPosition, 
            requiredSize, false, false, GUIStyle.none, GUIStyle.none);

        // Draw each ability item.
        for (int i = 0; i < abilities.Length; i++)
        {
            // Draw the ability background.
            if (engineEditor.engine == abilities[i].engine)
                EditorGUI.DrawRect(new Rect(posX, posY, width, itemHeight), selectedColor);
            else
                EditorGUI.DrawRect(new Rect(posX, posY, width, itemHeight), unselectedColor);

            // Draw the ability icon.
            EditorGUI.DrawRect(new Rect(posX, posY, itemHeight, itemHeight), Color.black);
            if (abilities[i].abilityIcon != null)
                GUI.DrawTexture(new Rect(posX, posY, itemHeight, itemHeight), abilities[i].abilityIcon.texture);

            Rect r = new Rect(posX + itemHeight + spacer, posY, width - itemHeight - spacer, itemHeight);

            if (EditorUtility.IsDirty(abilities[i]))
                GUI.Label(r, "*" + abilities[i].abilityName, LogicEngineEditor.windowStyle_BodyText);
            else
                GUI.Label(r, abilities[i].abilityName, LogicEngineEditor.windowStyle_BodyText);



            Event current = Event.current;
            if (r.Contains(current.mousePosition))
            {
                if (current.type == EventType.MouseDown)
                {
                    SetSelectedAbility(abilities[i]);
                    
                }
                if (current.type == EventType.ContextClick)
                {
                    SetSelectedAbility(abilities[i]);
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Copy"), false, () => { CopyItem(ability); });
                    menu.AddItem(new GUIContent("Rename"), false, () => { RenameItem(ability); });
                    menu.AddItem(new GUIContent("Delete"), false, () => { DeleteItem(ability); });
                    menu.ShowAsContext();
                    current.Use();
                }
            }
            posY += itemHeight + itemPadding;
        }

        // 
        GUI.EndScrollView();
        Event current2 = Event.current;
        if (panel.Contains(current2.mousePosition) && current2.type == EventType.MouseDown)
            GUI.FocusControl(null);
    }

    /// <summary>
    /// Create a new ability, from a given list of template abilities.
    /// </summary>
    private void CreateNewAbility()
    {
        GenericMenu menu = new GenericMenu();

        Ability[] abilities = Resources.LoadAll<Ability>(templatesResourceFolder);
        for (int i = 0; i < abilities.Length; i++)
        {
            Ability ab = abilities[i];
            menu.AddItem(new GUIContent(ab.abilityName), false, () => { CopyItem(ab); });
        }
        menu.ShowAsContext();
    }

    /// <summary>
    /// Create a copy of the given ability, saving it to file.
    /// </summary>
    public void CopyItem (Ability abilityToCopy)
    {
        string abilityName = "";
        abilityName = EditorInputDialog.Show("Enter New Name", "", abilityToCopy.abilityName);
        if (abilityName != null && abilityName.Length > 0)
        {
            Ability copy = abilityToCopy.Copy();
            copy.abilityName = abilityName;
            string path = AssetDatabase.GenerateUniqueAssetPath($"{fullAbilitiesFolderPath}/{abilityName}.asset");
            AssetDatabase.CreateAsset(copy, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SetSelectedAbility(copy);
            //ability = copy;
            //engineEditor.SetEngine(ability.engine);
        }
    }

    /// <summary>
    /// Create a prompt to rename the given ability.
    /// </summary>
    public void RenameItem (Ability abilityToRename)
    {
        string abilityName = "";
        abilityName = EditorInputDialog.Show("Enter New Name", "", abilityToRename.abilityName);
        if (abilityName != null && abilityName.Length > 0)
        {
            abilityToRename.abilityName = abilityName;
            EditorUtility.SetDirty(abilityToRename);
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(abilityToRename), abilityName);
            
            abilityToRename.name = abilityName;
        }
    }

    /// <summary>
    /// Delete the given ability.
    /// </summary>
    public void DeleteItem (Ability abilityToDelete)
    {
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(abilityToDelete));
    }



    private void DrawAbilityInspector (Rect panel)
    {
        // Draw the background panel color.
        EditorGUI.DrawRect(panel, panelColor);

        Color headerColor = new Color(0.1f, 0.1f, 0.1f, 1);
        Color boxColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        float boxPadding = 5;
        float posX = panel.x;
        float posY = panel.y;
        float width = panel.width;
        float headerHeight = 25;
        float iconSize = 70;

        EditorGUI.DrawRect(new Rect(posX, posY, width, headerHeight), headerColor);
        EditorGUI.LabelField(new Rect(posX + 8, posY, width, headerHeight), 
            "[Settings - " + ability.abilityName + "]", LogicEngineEditor.windowStyle_HeaderText);

        posY += headerHeight;


        posX += 10;
        posY += 10;
        width -= 20;

        // Ability Icon Option
        Rect iconRect = new Rect(posX, posY, iconSize, iconSize);
        ability.abilityIcon = (Sprite)EditorGUI.ObjectField(iconRect, ability.abilityIcon, typeof(Sprite), false);

        // Ability Type Option
        GUI.Label(new Rect(posX + 80, posY -5, 100, 20), "Ability Targets", EditorStyles.boldLabel);
        ability.targetMode = (Ability.TargetMode)EditorGUI.EnumPopup(new Rect(posX + 80, posY + 14, 130, 22), ability.targetMode);

        // Cast Animation Option
        GUI.Label(new Rect(posX + 80, posY + 34, 100, 20), "Cast Animation", EditorStyles.boldLabel);
        int id = 0;
        for (int i = 0; i < Unit.animations.Length; i++)
            if (Unit.animations[i] == ability.abilityAnimation)
                id = i;
        ability.abilityAnimation = Unit.animations[EditorGUI.Popup(new Rect(posX + 80, posY + 52, 130, 22), id, Unit.animations)];
        posY += 74;


        // Ability Description
        GUI.Label(new Rect(posX, posY, width, 20), "Ability Description", EditorStyles.boldLabel);
        posY += 20;
        ability.abilityDescription = GUI.TextArea(new Rect(posX, posY, width, 70), ability.abilityDescription);
        posY += 73;

        //
        GUI.Label(new Rect(posX, posY, width, 20), "Ability Flavour Text", EditorStyles.boldLabel);
        posY += 20;
        ability.abilityFlavourText = GUI.TextArea(new Rect(posX, posY, width, 40), ability.abilityFlavourText);
        
        posY += 44;

        GUI.Label(new Rect(posX, posY, width, 20), "Tag", EditorStyles.boldLabel);
        ability.abilityTag = GUI.TextField(new Rect(posX + 30, posY, width - 30, 20), ability.abilityTag);
        posY += 29;


        EditorGUI.DrawRect(new Rect(posX, posY, width, 130), boxColor);

        posY += boxPadding;
        ability.canMoveWhileCasting = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), ability.canMoveWhileCasting, " Can Cast While Moving");
        posY += 20;
        ability.hasSpecificCastTime = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), ability.hasSpecificCastTime, " Has Specific Cast Time");
        posY += 20;
        
        ability.requiresLineOfSight = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), ability.requiresLineOfSight, " Requires Line of Sight");
        posY += 20;
        ability.canUpdateTargetWhileCasting = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), ability.canUpdateTargetWhileCasting, " Update Target While Casting");
        posY += 20;

        ability.cooldownIsAtackSpeed = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), ability.cooldownIsAtackSpeed, " Cooldown is Attack Speed");
        posY += 20;
        ability.abilityGeneratesResource = GUI.Toggle(new Rect(posX + boxPadding, posY, width, 22), ability.abilityGeneratesResource, " Ability Generates Resource");
        posY += 34;



        EditorGUI.DrawRect(new Rect(posX, posY, width, 94), boxColor);
        posY += 2;
        
        GUI.Label(new Rect(posX + boxPadding, posY, 100, 20), "Range", EditorStyles.boldLabel);
        if (ability.targetMode == Ability.TargetMode.PointInMelee ||
            ability.targetMode == Ability.TargetMode.UnitInMelee)
        {
            GUI.enabled = false;
            EditorGUI.TextField(new Rect(posX + boxPadding, posY + 20, 100, 20), "Melee");
            GUI.enabled = true;
        }
        else if (ability.targetMode == Ability.TargetMode.None)
        {
            GUI.enabled = false;
            EditorGUI.TextField(new Rect(posX + boxPadding, posY + 20, 100, 20), "N/A");
            GUI.enabled = true;
        }
        else
        {
            ability.range = EditorGUI.FloatField(new Rect(posX + boxPadding, posY + 20, 100, 20), ability.range);
        }

        if (ability.abilityGeneratesResource)
        {
            GUI.Label(new Rect(posX + 110, posY, 95, 20), "Resource Gain", EditorStyles.boldLabel);
        }
        else
        {
            GUI.Label(new Rect(posX + 110, posY, 95, 20), "Resource Cost", EditorStyles.boldLabel);
        }
        ability.abilityCost = EditorGUI.FloatField(new Rect(posX + 110, posY + 20, 95, 20), ability.abilityCost);
        ability.abilityCost = Mathf.Max(0, ability.abilityCost);

        posY += 44;

        GUI.Label(new Rect(posX + boxPadding, posY, 100, 20), "Cast Time", EditorStyles.boldLabel);
        if (ability.hasSpecificCastTime)
        {
            ability.castTime = EditorGUI.FloatField(new Rect(posX + boxPadding, posY + 20, 100, 20), ability.castTime);
        }
        else
        {
            GUI.enabled = false;
            EditorGUI.TextField(new Rect(posX + boxPadding, posY + 20, 100, 20), "Auto");
            GUI.enabled = true;
        }

        GUI.Label(new Rect(posX + 110, posY, 95, 20), "Cooldown", EditorStyles.boldLabel);
        if (!ability.cooldownIsAtackSpeed)
        {
            
            ability.abilityCooldown = EditorGUI.FloatField(new Rect(posX + 110, posY + 20, 95, 20), ability.abilityCooldown);
        }
        else
        {
            GUI.enabled = false;
            EditorGUI.TextField(new Rect(posX + 110, posY + 20, 95, 20), "Attack Speed");
            GUI.enabled = true;
        }
        

        posY += 56;

        GUI.Label(new Rect(posX, posY, 150, 20), "Ability Sound Effect", EditorStyles.boldLabel);
        posY += 22;
        ability.castCompleteSound = (AudioClip)EditorGUI.ObjectField(new Rect(posX, posY, width, 22), ability.castCompleteSound, typeof(AudioClip), false);
        posY += 22;
        GUI.Label(new Rect(posX, posY, 50, 20), "Volume");
        posY += 3;
        ability.castCompleteSoundVolume = EditorGUI.Slider(new Rect(posX + 55, posY, width - 55, 20), ability.castCompleteSoundVolume, 0.0f, 1.0f);
        posY += 24;



        GUI.Label(new Rect(posX, posY, 150, 20), "Animation Trigger Point", EditorStyles.boldLabel);
        posY += 22;
        ability.animationActivationPosition = EditorGUI.Slider(new Rect(posX, posY, width, 20), ability.animationActivationPosition, 0.0f, 1.0f);
        posY += 20;
    }

    /// <summary>
    /// Handle all drawing for the entire UI window.
    /// </summary>
    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();

        if (ability == null || engineEditor == null) 
        {
            ability = Resources.LoadAll<Ability>(resourceSubFolder)[0];
            if (ability == null) return;
            engineEditor = new LogicEngineEditor(this, ability.engine, ability);
        }

        // Record the state before any changes
        Undo.RecordObject(ability, "Changed Ability " + ability.abilityName);

        float spacer = 4;
        float posX = spacer;
        float posY = spacer;

        // Draw the list of all abilities in the project.
        DrawAbilityList(new Rect(posX, posY, abilityListPanelWidth, position.height - 2 * spacer));
        posX += abilityListPanelWidth + spacer;

        // Draw the details for the selected ability.
        DrawAbilityInspector(new Rect(posX, posY, abilityDetailsPanelWidth, position.height - 2 * spacer));
        posX += abilityDetailsPanelWidth + spacer;

        // Draw the scripting logic for the selected ability.
        engineEditor.Process();
        engineEditor.DrawNodes(new Rect(posX, posY, this.position.width - posX - spacer, this.position.height - 2 * spacer));

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(ability);
        }
    }

    /// <summary>
    /// Constantly repaint the inspector (OnInspectorUpdate only updates 10 times per second).
    /// </summary>
    private void OnInspectorUpdate()
    {
        if (this.IsFocused())
            Repaint();
    }
}
