import { useEffect } from "react";
import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue, call } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { tool } from "cs2/bindings";
import locale from "../lang/en-US.json";
import { getModule } from "cs2/modding";
import { Button } from "cs2/ui";
import marqueeToolSrc from "../../img/icon_Marquee_Off.svg";
import marqueeToolActiveSrc from "../../img/icon_Marquee_Active.svg";

// These establishes the binding with C# side. Without C# side game ui will crash.
const raycastTarget$ = bindValue<number>(mod.id, 'RaycastTarget');
const isGame$ = bindValue<boolean>(mod.id, 'IsGame');
const selectedVanillaFilters$ = bindValue<VanillaFilters>(mod.id, "SelectedVanillaFilters");
const isMarqueeToolSelected$ = bindValue<boolean>(mod.id, "IsMarqueeToolSelected");

// These contain the coui paths to Unified Icon Library svg assets
const uilStandard =                         "coui://uil/Standard/";

const allSrc =              uilStandard + "StarAll.svg";
const networkSrc =         uilStandard +  "Network.svg";
const decalsSrc =           uilStandard +  "Decals.svg";
const treeSrc =           uilStandard +  "TreeAdult.svg";
const plantSrc =           uilStandard +  "FlowerPot.svg";
const buildingSrc =         uilStandard + "House.svg";
const propsSrc =            uilStandard + "BenchAndLampProps.svg"; 

// Saving strings for events and translations.
const tooltipDescriptionPrefix ="LAYERED_SELECTION_DISPLAY_DESCRIPTION.";
const sectionTitlePrefix =      "LAYERED_SELECTION_DISPLAY.";
const toolsSectionTitle =          "LAYERED_SELECTION_DISPLAY_MAINPANEL.Tools";
const marqueeToolTooltip =          "LAYERED_SELECTION_DISPLAY_MAINPANEL.MarqueeToolToolTip";

// This functions trigger an event on C# side and C# designates the method to implement.
function handleClick(eventName: string) {
    trigger(mod.id, eventName);
}

// This functions trigger an event on C# side and C# designates the method to implement.
function changeSelectedVanillaFilter(filter: VanillaFilters) {
    trigger(mod.id, "ChangeVanillaFilter", filter);
}

function onChangeListPanelVisibility() {
    trigger(mod.id, "OnChangeListPanelVisibility");
}

enum VanillaFilters 
{
    None = 0,
    Networks = 1,
    Buildings = 2,
    Trees = 4,
    Plants = 8,
    Decals = 16,
    Props = 32,
    Surfaces = 64,
    All = 128,
}

const descriptionToolTipStyle = getModule("game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss", "classes");    

// This is working, but it's possible a better solution is possible.
function descriptionTooltip(tooltipTitle: string | null, tooltipDescription: string | null) : JSX.Element {
    return (
        <>
            <div className={descriptionToolTipStyle.title}>{tooltipTitle}</div>
            <div className={descriptionToolTipStyle.content}>{tooltipDescription}</div>
        </>
    );
}

export const LayeredSelectionDisplaySectionsComponent: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};

        // These get the value of the bindings.
        const defaultToolActive = useValue(tool.activeTool$).id == tool.DEFAULT_TOOL;
        const selectedVanillaFilters = useValue(selectedVanillaFilters$);
        const isGame = useValue(isGame$);
        const raycastTarget = useValue(raycastTarget$);
        const isMarqueeToolSelected = useValue(isMarqueeToolSelected$);
        const marqueeToolIcon = isMarqueeToolSelected ? marqueeToolActiveSrc : marqueeToolSrc;
        
        // Saving strings for events and translations.
        const surfacesID =              "SurfacesFilterButton";
                              
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        const filterSectionTitle =          translate(sectionTitlePrefix + "Filter",                        locale["LAYERED_SELECTION_DISPLAY.Filter"]);
        const surfacesFilterTooltip =       translate(tooltipDescriptionPrefix + surfacesID,                locale["LAYERED_SELECTION_DISPLAY_DESCRIPTION.SurfacesFilterButton"]);        
        const toolModeTitle =               translate("Toolbar.TOOL_MODE_TITLE", "Tool Mode");        
        const surfacesSrc =                     uilStandard + "ShovelSurface.svg";
        const surfacesFilterTitle =         translate("LayeredSelectionDisplay.TOOLTIP_TITLE[SurfacesFilterButton]" );
        const allFiltersTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[AllFilters]" ,locale["LayeredSelectionDisplay.TOOLTIP_TITLE[AllFilters]"]);
        const allFiltersDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[AllFilters]" ,locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[AllFilters]"]);
        const vanillaNetworksFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[VanillaNetworksFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_TITLE[VanillaNetworksFilter]"]);
        const vanillaNetworksFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaNetworksFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaNetworksFilter]"]);
        const buildingFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[BuildingFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_TITLE[BuildingFilter]"]);
        const buildingFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[BuildingFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[BuildingFilter]"]);
        const treeFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[TreeFilter]" , locale["LayeredSelectionDisplay.TOOLTIP_TITLE[TreeFilter]"]);
        const treeFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[TreeFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[TreeFilter]"]);
        const plantFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[PlantFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_TITLE[PlantFilter]"]);
        const plantFilterDescription = translate( "LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PlantFilter]",locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PlantFilter]"]);
        const decalFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[DecalFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_TITLE[DecalFilter]"]);
        const decalFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[DecalFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[DecalFilter]"]);
        const propFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[PropFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_TITLE[PropFilter]"]);
        const propFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PropFilter]" ,locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PropFilter]"]);
        const vanillaSurfaceFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaSurfaceFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaSurfaceFilter]"]);
        const toolsSectionTitle = translate("LAYERED_SELECTION_DISPLAY_MAINPANEL.Tools", locale["LAYERED_SELECTION_DISPLAY_MAINPANEL.Tools"]);
        const marqueeToolTooltip = translate("LAYERED_SELECTION_DISPLAY_MAINPANEL.MarqueeToolToolTip", locale["LAYERED_SELECTION_DISPLAY_MAINPANEL.MarqueeToolToolTip"]);


        // This gets the original component that we may alter and return.
        var result: JSX.Element = Component();        
        // It is important that we coordinate how to handle the tool options panel because it is possibile to create a mod that works for your mod but prevents others from doing the same thing.
        if (defaultToolActive && isGame) {
            result.props.children?.push(
                /* 
                All properties of the buttons and sections have been previously defined in variables above.
                */
                <>
                    { raycastTarget == 0 && (   
                        // This section is only showing while using vanilla bulldozer.
                        <>  
                            <VanillaComponentResolver.instance.Section title={toolsSectionTitle}>
                                <VanillaComponentResolver.instance.ToolButton  onSelect={() => onChangeListPanelVisibility()}     tooltip={marqueeToolTooltip}          src={marqueeToolIcon}          className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}></VanillaComponentResolver.instance.ToolButton>
                            </VanillaComponentResolver.instance.Section>
                            <VanillaComponentResolver.instance.Section title={filterSectionTitle}> 
                                <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.All) == VanillaFilters.All}               tooltip={descriptionTooltip(allFiltersTitle ,allFiltersDescription)}                        src={allSrc}            onSelect={() => changeSelectedVanillaFilter(VanillaFilters.All)}        className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Buildings) == VanillaFilters.Buildings}   tooltip={descriptionTooltip(buildingFilterTitle ,buildingFilterDescription)}                src={buildingSrc}       onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Buildings)}  className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Trees) == VanillaFilters.Trees}           tooltip={descriptionTooltip(treeFilterTitle ,treeFilterDescription)}                        src={treeSrc}           onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Trees)}      className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Plants) == VanillaFilters.Plants}         tooltip={descriptionTooltip(plantFilterTitle ,plantFilterDescription)}                      src={plantSrc}          onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Plants)}     className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Decals) == VanillaFilters.Decals}         tooltip={descriptionTooltip(decalFilterTitle ,decalFilterDescription)}                      src={decalsSrc}         onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Decals)}     className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                                <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Props) == VanillaFilters.Props}           tooltip={descriptionTooltip(propFilterTitle ,propFilterDescription)}                        src={propsSrc}          onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Props)}      className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                            </VanillaComponentResolver.instance.Section>   
                        </>      
                    )}
                </>
            );                   
        }

        return result;
    };
}