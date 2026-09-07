import * as React from "react";
import { useEffect, useLayoutEffect, useRef, useState } from "react";
import { useLocalization } from "cs2/l10n";
import { ModuleRegistryExtend } from "cs2/modding";
import { bindValue, trigger, useValue } from "cs2/api";
import { VanillaComponentResolver } from "../VanillaComponentResolver/VanillaComponentResolver";
import mod from "../../../mod.json";
import locale from "../lang/en-US.json";
import { getModule } from "cs2/modding";
import { Tooltip } from "cs2/ui";
import marqueeToolSrc from "../../img/icon_Marquee_Off.svg";
import marqueeToolActiveSrc from "../../img/icon_Marquee_Active.svg";

import styles from "./LayeredSelectionDisplaySections.module.scss";

// These establishes the binding with C# side. Without C# side game ui will crash.
const raycastTarget$ = bindValue<number>(mod.id, "RaycastTarget");
const isGame$ = bindValue<boolean>(mod.id, "IsGame");
const selectedVanillaFilters$ = bindValue<VanillaFilters>(mod.id, "SelectedVanillaFilters");
const isMarqueeToolSelected$ = bindValue<boolean>(mod.id, "IsMarqueeToolSelected");
const isFiltersPanelVisible$ = bindValue<boolean>(mod.id, "IsFiltersPanelVisible");

// These contain the coui paths to Unified Icon Library svg assets
const uilStandard = "coui://uil/Standard/";

const allSrc = uilStandard + "StarAll.svg";
const networkSrc = uilStandard + "Network.svg";
const decalsSrc = uilStandard + "Decals.svg";
const treeSrc = uilStandard + "TreeAdult.svg";
const plantSrc = uilStandard + "FlowerPot.svg";
const buildingSrc = uilStandard + "House.svg";
const propsSrc = uilStandard + "BenchAndLampProps.svg";

const isDefaultToolActive$ = bindValue<boolean>(mod.id, "IsDefaultToolActive");

// Saving strings for events and translations.
const tooltipDescriptionPrefix =
    "LAYERED_SELECTION_DISPLAY_DESCRIPTION.";
const sectionTitlePrefix =
    "LAYERED_SELECTION_DISPLAY.";
const toolsSectionTitle =
    "LAYERED_SELECTION_DISPLAY_MAINPANEL.Tools";
const marqueeToolTooltip =
    "LAYERED_SELECTION_DISPLAY_MAINPANEL.MarqueeToolToolTip";

function onChangeFiltersPanelVisibility() {
    // console.log("Toggling filters panel visibility.");
    trigger(mod.id, "OnChangeFiltersPanelVisibility");
}

function changeSelectedVanillaFilter(filter: VanillaFilters) {
    trigger(mod.id, "ChangeVanillaFilter", filter);
}

function onChangeListPanelVisibility() {
    trigger(mod.id, "OnChangeListPanelVisibility");
}

enum VanillaFilters {
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

const descriptionToolTipStyle = getModule(
    "game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss",
    "classes"
);

// This is working, but it's possible a better solution is possible.
function descriptionTooltip(
    tooltipTitle: string | null,
    tooltipDescription: string | null
): JSX.Element {
    return (
        <>
            <div className={descriptionToolTipStyle.title}>
                {tooltipTitle}
            </div>

            <div className={descriptionToolTipStyle.content}>
                {tooltipDescription}
            </div>
        </>
    );
}

export const LayeredSelectionDisplaySectionsComponent: ModuleRegistryExtend =
    (Component: any) => {
        return (props) => {

            const { children, ...otherProps } = props || {};
            
            const defaultToolActive = useValue(isDefaultToolActive$);
            const selectedVanillaFilters = useValue(selectedVanillaFilters$);
            const isGame = useValue(isGame$);
            const raycastTarget = useValue(raycastTarget$);
            const isMarqueeToolSelected = useValue(isMarqueeToolSelected$);
            const marqueeToolIcon = isMarqueeToolSelected ? marqueeToolActiveSrc : marqueeToolSrc;
            const isFiltersPanelVisible = useValue(isFiltersPanelVisible$);

            // Forces a re-render only when tool-panel presence actually flips —
            // not on every internal DOM churn (that was the earlier crash cause).
            const [, forceRerender] = useState(0);
            const lastToolPanelPresentRef = useRef<boolean | null>(null);

            useEffect(() => {
                const checkForChange = () => {
                    const present = defaultToolActive && isGame && !!document.querySelector(".tool-panel_V_j");
                    if (lastToolPanelPresentRef.current !== present) {
                        lastToolPanelPresentRef.current = present;
                        console.log("Tool panel presence changed:", present);
                        forceRerender((n) => n + 1);
                    }
                };

                checkForChange();

                // console.log("Initial tool panel presence:", lastToolPanelPresentRef.current);
                // console.log("isFiltersPanelVisible:", isFiltersPanelVisible);
                const toolLayout = document.querySelector(".tool-layout_SqM");
                let observer: MutationObserver | null = null;
                if (toolLayout) {
                    observer = new MutationObserver(checkForChange);
                    observer.observe(toolLayout, { childList: true, subtree: true });
                }

                return () => observer?.disconnect();
            }, [defaultToolActive, isGame, isFiltersPanelVisible]);

            const toolPanelPresent = defaultToolActive && isGame && !!document.querySelector(".tool-panel_V_j");

            useLayoutEffect(() => {
                const wrapper = document.querySelector(".wrapper_eKY") as HTMLElement | null;
                if (!wrapper) {
                    return;
                }

                const clear = () => {
                    wrapper.style.removeProperty("width");
                    wrapper.style.removeProperty("height");
                    wrapper.style.removeProperty("min-width");
                    wrapper.style.removeProperty("min-height");
                    wrapper.style.removeProperty("cursor");
                    wrapper.style.removeProperty("overflow");
                    wrapper.style.removeProperty("transition");
                };

                const applyMinimized = () => {
                    wrapper.style.setProperty("transition", "none", "important");
                    wrapper.style.setProperty("width", "70rem", "important");
                    wrapper.style.setProperty("height", "15rem", "important");
                    wrapper.style.setProperty("min-width", "70rem", "important");
                    wrapper.style.setProperty("min-height", "15rem", "important");
                    wrapper.style.setProperty("overflow", "hidden", "important");
                };

                // console.log("Default tool active:", defaultToolActive);
                // console.log("Game active:", isGame);
                // console.log("Filters panel visible:", isFiltersPanelVisible);
                // console.log("Tool panel present:", toolPanelPresent);

                if (((!defaultToolActive) || (defaultToolActive && isFiltersPanelVisible)) || toolPanelPresent  || !isGame) {
                    // console.log("Default tool is not active or game is not active or filters panel is visible or tool panel is present.");
                    clear();
                    wrapper.onclick = null;
                    return;
                }

                if (isFiltersPanelVisible) {
                    // console.log("Filters panel is visible.");
                    clear();
                } else if (toolPanelPresent) {
                    // console.log("Tool panel is present.");
                    clear();
                } else {
                    applyMinimized();
                }

                wrapper.style.setProperty("cursor", "pointer", "important");
                wrapper.onclick = () => onChangeFiltersPanelVisibility();
            });

            // Saving strings for events and translations.
            const surfacesID = "SurfacesFilterButton";

            // Translation handling.
            const { translate } = useLocalization();

            const filterSectionTitle = translate(sectionTitlePrefix + "Filter", locale["LAYERED_SELECTION_DISPLAY.Filter"]);
            const hidePanelToolTip = translate("LAYERED_SELECTION_DISPLAY_MAINPANEL.HidePanelToolTip", locale["LAYERED_SELECTION_DISPLAY_MAINPANEL.HidePanelToolTip"]);
            const showPanelToolTip = translate("LAYERED_SELECTION_DISPLAY_MAINPANEL.ShowPanelToolTip", locale["LAYERED_SELECTION_DISPLAY_MAINPANEL.ShowPanelToolTip"]);
            const surfacesFilterTooltip = translate(tooltipDescriptionPrefix + surfacesID,locale["LAYERED_SELECTION_DISPLAY_DESCRIPTION.SurfacesFilterButton"]);
            const toolModeTitle = translate("Toolbar.TOOL_MODE_TITLE","Tool Mode");
            const surfacesSrc = uilStandard + "ShovelSurface.svg";
            const surfacesFilterTitle =translate("LayeredSelectionDisplay.TOOLTIP_TITLE[SurfacesFilterButton]");
            const allFiltersTitle =translate("LayeredSelectionDisplay.TOOLTIP_TITLE[AllFilters]", locale["LayeredSelectionDisplay.TOOLTIP_TITLE[AllFilters]"]);
            const allFiltersDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[AllFilters]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[AllFilters]"]);
            const vanillaNetworksFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[VanillaNetworksFilter]", locale["LayeredSelectionDisplay.TOOLTIP_TITLE[VanillaNetworksFilter]"]);
            const vanillaNetworksFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaNetworksFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaNetworksFilter]"]);
            const buildingFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[BuildingFilter]", locale["LayeredSelectionDisplay.TOOLTIP_TITLE[BuildingFilter]"]);
            const buildingFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[BuildingFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[BuildingFilter]"]);
            const treeFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[TreeFilter]", locale["LayeredSelectionDisplay.TOOLTIP_TITLE[TreeFilter]"]);
            const treeFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[TreeFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[TreeFilter]"]);
            const plantFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[PlantFilter]", locale["LayeredSelectionDisplay.TOOLTIP_TITLE[PlantFilter]"]);
            const plantFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PlantFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PlantFilter]"]);
            const decalFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[DecalFilter]", locale["LayeredSelectionDisplay.TOOLTIP_TITLE[DecalFilter]"]);
            const decalFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[DecalFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[DecalFilter]"]);
            const propFilterTitle = translate("LayeredSelectionDisplay.TOOLTIP_TITLE[PropFilter]", locale["LayeredSelectionDisplay.TOOLTIP_TITLE[PropFilter]"]);
            const propFilterDescription = translate("LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PropFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[PropFilter]"]);
            const vanillaSurfaceFilterDescription = translate("LAYERED_SELECTION_DISPLAY.TOOLTIP_DESCRIPTION[VanillaSurfaceFilter]", locale["LayeredSelectionDisplay.TOOLTIP_DESCRIPTION[VanillaSurfaceFilter]"]);
            const toolsSectionTitleTranslated = translate("LAYERED_SELECTION_DISPLAY_MAINPANEL.Tools", locale["LAYERED_SELECTION_DISPLAY_MAINPANEL.Tools"]);
            const marqueeToolTooltipTranslated = translate("LAYERED_SELECTION_DISPLAY_MAINPANEL.MarqueeToolToolTip", locale["LAYERED_SELECTION_DISPLAY_MAINPANEL.MarqueeToolToolTip"]);

            // ORIGINAL COMPONENT
            const result: JSX.Element = Component();

            // console.log("Default tool active:", defaultToolActive);
            // console.log("Game active:", isGame);
            // console.log("Filters panel visible:", isFiltersPanelVisible);
            // console.log("Tool panel present:", toolPanelPresent);
            
            // OPEN STATE — only inject our Tools/Filter sections when vanilla isn't
            // already using this shared container for its own tool options.
            if (defaultToolActive && isGame && isFiltersPanelVisible) {
                if (!toolPanelPresent) {
                    // Full Tools + Filter sections
                    result.props.children?.push(
                        <>
                            {raycastTarget == 0 && (
                                <>
                                    <VanillaComponentResolver.instance.Section
                                        title={
                                            toolsSectionTitleTranslated ?? ""
                                        }
                                    >
                                        <VanillaComponentResolver.instance.ToolButton
                                            onSelect={() =>
                                                onChangeListPanelVisibility()
                                            }
                                            tooltip={
                                                marqueeToolTooltipTranslated
                                            }
                                            src={marqueeToolIcon}
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />

                                        <VanillaComponentResolver.instance.ToolButton
                                            onSelect={() => onChangeFiltersPanelVisibility() }
                                            src="coui://uil/Standard/ArrowsMinimizeBold.svg"
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />
                                    </VanillaComponentResolver.instance.Section>

                                    <VanillaComponentResolver.instance.Section
                                        title={
                                            filterSectionTitle ?? ""
                                        }
                                    >
                                        <VanillaComponentResolver.instance.ToolButton
                                            selected={
                                                (selectedVanillaFilters &
                                                    VanillaFilters.All) ==
                                                VanillaFilters.All
                                            }
                                            tooltip={descriptionTooltip(
                                                allFiltersTitle,
                                                allFiltersDescription
                                            )}
                                            src={allSrc}
                                            onSelect={() =>
                                                changeSelectedVanillaFilter(
                                                    VanillaFilters.All
                                                )
                                            }
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />

                                        <VanillaComponentResolver.instance.ToolButton
                                            selected={
                                                (selectedVanillaFilters &
                                                    VanillaFilters.Buildings) ==
                                                VanillaFilters.Buildings
                                            }
                                            tooltip={descriptionTooltip(
                                                buildingFilterTitle,
                                                buildingFilterDescription
                                            )}
                                            src={buildingSrc}
                                            onSelect={() =>
                                                changeSelectedVanillaFilter(
                                                    VanillaFilters.Buildings
                                                )
                                            }
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />

                                        <VanillaComponentResolver.instance.ToolButton
                                            selected={
                                                (selectedVanillaFilters &
                                                    VanillaFilters.Trees) ==
                                                VanillaFilters.Trees
                                            }
                                            tooltip={descriptionTooltip(
                                                treeFilterTitle,
                                                treeFilterDescription
                                            )}
                                            src={treeSrc}
                                            onSelect={() =>
                                                changeSelectedVanillaFilter(
                                                    VanillaFilters.Trees
                                                )
                                            }
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />

                                        <VanillaComponentResolver.instance.ToolButton
                                            selected={
                                                (selectedVanillaFilters &
                                                    VanillaFilters.Plants) ==
                                                VanillaFilters.Plants
                                            }
                                            tooltip={descriptionTooltip(
                                                plantFilterTitle,
                                                plantFilterDescription
                                            )}
                                            src={plantSrc}
                                            onSelect={() =>
                                                changeSelectedVanillaFilter(
                                                    VanillaFilters.Plants
                                                )
                                            }
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />

                                        <VanillaComponentResolver.instance.ToolButton
                                            selected={
                                                (selectedVanillaFilters &
                                                    VanillaFilters.Decals) ==
                                                VanillaFilters.Decals
                                            }
                                            tooltip={descriptionTooltip(
                                                decalFilterTitle,
                                                decalFilterDescription
                                            )}
                                            src={decalsSrc}
                                            onSelect={() =>
                                                changeSelectedVanillaFilter(
                                                    VanillaFilters.Decals
                                                )
                                            }
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />

                                        <VanillaComponentResolver.instance.ToolButton
                                            selected={
                                                (selectedVanillaFilters &
                                                    VanillaFilters.Props) ==
                                                VanillaFilters.Props
                                            }
                                            tooltip={descriptionTooltip(
                                                propFilterTitle,
                                                propFilterDescription
                                            )}
                                            src={propsSrc}
                                            onSelect={() =>
                                                changeSelectedVanillaFilter(
                                                    VanillaFilters.Props
                                                )
                                            }
                                            className={
                                                VanillaComponentResolver
                                                    .instance
                                                    .toolButtonTheme.button
                                            }
                                            focusKey={
                                                VanillaComponentResolver
                                                    .instance
                                                    .FOCUS_DISABLED
                                            }
                                        />
                                    </VanillaComponentResolver.instance.Section>
                                </>
                            )}
                        </>
                    );                
                }
            }
            
            let chld = React.Children.toArray(result.props.children);

            // CLOSED STATE — the minimized toggle should still show alongside
            // vanilla's own content (that part already worked correctly before).
            if (defaultToolActive && isGame && !isFiltersPanelVisible && !toolPanelPresent) {
                chld.push(
                    <Tooltip key="lsd-minimized-tooltip" tooltip={showPanelToolTip}>
                        <div className={styles.minimizedPanel}>
                            <span className={styles.toggleIco} />
                        </div>
                    </Tooltip>
                );
            }

            result.props.children = chld;

            return result;
        };
    };