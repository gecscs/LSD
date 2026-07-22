import { useEffect } from "react";
import { useLocalization } from "cs2/l10n";
import {ModuleRegistryExtend} from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import { tool } from "cs2/bindings";
import locale from "../lang/en-US.json";
import { getModule } from "cs2/modding";

// These establishes the binding with C# side. Without C# side game ui will crash.
const raycastTarget$ = bindValue<number>(mod.id, 'RaycastTarget');
const isGame$ = bindValue<boolean>(mod.id, 'IsGame');
const selectedVanillaFilters$ = bindValue<VanillaFilters>(mod.id, "SelectedVanillaFilters");


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
const tooltipDescriptionPrefix ="LSD_LAYERED_SELECTION_DISPLAY_DESCRIPTION.";
const sectionTitlePrefix =      "LSD_LAYERED_SELECTION_DISPLAY.";

// This functions trigger an event on C# side and C# designates the method to implement.
function handleClick(eventName: string) {
    trigger(mod.id, eventName);
}

// This functions trigger an event on C# side and C# designates the method to implement.
function changeSelectedVanillaFilter(filter: VanillaFilters) {
    trigger(mod.id, "ChangeVanillaFilter", filter);
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

export const LSDLayeredSelectionDisplayComponent: ModuleRegistryExtend = (Component : any) => {
    // I believe you should not put anything here.
    return (props) => {
        // This defines aspects of the components.
        const {children, ...otherProps} = props || {};

        // These get the value of the bindings.
        //const subElementBulldozerToolActive = useValue(subElementBulldozeToolActive$);
        const bulldozeToolActive = useValue(tool.activeTool$).id == tool.DEFAULT_TOOL;
        const selectedVanillaFilters = useValue(selectedVanillaFilters$);
        const isGame = useValue(isGame$);
        const raycastTarget = useValue(raycastTarget$);
        
        // Saving strings for events and translations.
        const surfacesID =              "SurfacesFilterButton";
                              
        // translation handling. Translates using locale keys that are defined in C# or fallback string here.
        const { translate } = useLocalization();
        const filterSectionTitle =          translate(sectionTitlePrefix + "Filter",                        locale["LSD_LAYERED_SELECTION_DISPLAY.Filter"]);
        const surfacesFilterTooltip =       translate(tooltipDescriptionPrefix + surfacesID,                locale["LSD_LAYERED_SELECTION_DISPLAY_DESCRIPTION.SurfacesFilterButton"]);
        
        const toolModeTitle =               translate("Toolbar.TOOL_MODE_TITLE", "Tool Mode");
        
        const surfacesSrc =                     uilStandard + "ShovelSurface.svg";

        const surfacesFilterTitle =         translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[SurfacesFilterButton]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[SurfacesFilterButton]"]);
        const allFiltersTitle = translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[AllFilters]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[AllFilters]"]);
        const allFiltersDescription = translate("LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[AllFilters]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[AllFilters]"]);
        const vanillaNetworksFilterTitle = translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[VanillaNetworksFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[VanillaNetworksFilter]"]);
        const vanillaNetworksFilterDescription = translate("LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaNetworksFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaNetworksFilter]"]);
        const buildingFilterTitle = translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[BuildingFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[BuildingFilter]"]);
        const buildingFilterDescription = translate("LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[BuildingFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[BuildingFilter]"]);
        const treeFilterTitle = translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[TreeFilter]" , locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[TreeFilter]"]);
        const treeFilterDescription = translate("LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[TreeFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[TreeFilter]"]);
        const plantFilterTitle = translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[PlantFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[PlantFilter]"]);
        const plantFilterDescription = translate( "LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PlantFilter]",locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PlantFilter]"]);
        const decalFilterTitle = translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[DecalFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[DecalFilter]"]);
        const decalFilterDescription = translate("LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[DecalFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[DecalFilter]"]);
        const propFilterTitle = translate("LSDLayeredSelectionDisplay.TOOLTIP_TITLE[PropFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_TITLE[PropFilter]"]);
        const propFilterDescription = translate("LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PropFilter]" ,locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PropFilter]"]);
        const vanillaSurfaceFilterDescription = translate("LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaSurfaceFilter]", locale["LSDLayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaSurfaceFilter]"]);


        // This gets the original component that we may alter and return.
        var result: JSX.Element = Component();        
        // It is important that we coordinate how to handle the tool options panel because it is possibile to create a mod that works for your mod but prevents others from doing the same thing.
        // If bulldoze tool active add better bulldozer sections.
        if (bulldozeToolActive && isGame) {
            result.props.children?.push(
                /* 
                All properties of the buttons and sections have been previously defined in variables above.
                */
                <>
                    { raycastTarget == 0 && (                    
                        // This section is only showing while using vanilla bulldozer.
                        <VanillaComponentResolver.instance.Section title={filterSectionTitle}>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.All) == VanillaFilters.All}               tooltip={descriptionTooltip(allFiltersTitle ,allFiltersDescription)}                        src={allSrc}            onSelect={() => changeSelectedVanillaFilter(VanillaFilters.All)}        className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Surfaces) == VanillaFilters.Surfaces}     tooltip={descriptionTooltip(surfacesFilterTitle, vanillaSurfaceFilterDescription)}          src={surfacesSrc}       onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Surfaces)}   className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Networks) == VanillaFilters.Networks}     tooltip={descriptionTooltip(vanillaNetworksFilterTitle ,vanillaNetworksFilterDescription)}  src={networkSrc}        onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Networks)}   className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Buildings) == VanillaFilters.Buildings}   tooltip={descriptionTooltip(buildingFilterTitle ,buildingFilterDescription)}                src={buildingSrc}       onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Buildings)}  className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Trees) == VanillaFilters.Trees}           tooltip={descriptionTooltip(treeFilterTitle ,treeFilterDescription)}                        src={treeSrc}           onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Trees)}      className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Plants) == VanillaFilters.Plants}         tooltip={descriptionTooltip(plantFilterTitle ,plantFilterDescription)}                      src={plantSrc}          onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Plants)}     className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Decals) == VanillaFilters.Decals}         tooltip={descriptionTooltip(decalFilterTitle ,decalFilterDescription)}                      src={decalsSrc}         onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Decals)}     className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                            <VanillaComponentResolver.instance.ToolButton  selected={(selectedVanillaFilters & VanillaFilters.Props) == VanillaFilters.Props}           tooltip={descriptionTooltip(propFilterTitle ,propFilterDescription)}                        src={propsSrc}          onSelect={() => changeSelectedVanillaFilter(VanillaFilters.Props)}      className={VanillaComponentResolver.instance.toolButtonTheme.button} focusKey={VanillaComponentResolver.instance.FOCUS_DISABLED}     ></VanillaComponentResolver.instance.ToolButton>
                        </VanillaComponentResolver.instance.Section>                    
                    )}
                </>
            );                   
        }

        return result;
    };
}