import { useState } from "react";
import { useLocalization } from "cs2/l10n";
import { Panel, Portal, Scrollable, Button, Tooltip } from "cs2/ui";
import { bindValue, trigger, useValue } from "cs2/api";

import locale from "../mods/lang/en-US.json";
import styles from "./selectionListPanel.module.scss";
import mod from "../../mod.json";

import { SelectedEntities } from "../Domain/SelectedEntities";
import { SelectedEntity } from "../Domain/SelectedEntity";
import classNames from "classnames";

import marqueeToolSrc from "../img/icon_Marquee_Off.svg";
import marqueeToolActiveSrc from "../img/icon_Marquee_Active.svg";


const isGame$ = bindValue<boolean>(
    mod.id,
    "IsGame"
);

const isMoveItInstalled$ = bindValue<boolean>(
    mod.id,
    "IsMoveItInstalled"
);

const isMarqueeToolSelected$ = bindValue<boolean>(
    mod.id,
    "IsMarqueeToolSelected"
);

const isMarqueeToolActive$ = bindValue<boolean>(
    mod.id,
    "IsMarqueeToolActive"
);

const selectedEntities$ = bindValue<SelectedEntities>(
    mod.id,
    "SelectedEntities",
    { Entities: [] }
);

const panelPosition$ = bindValue(
    mod.id,
    "PanelPosition",
    { x: 0.5, y: 0.5 }
);

const expandedListPanel$ = bindValue(mod.id, "ExpandedListPanel", false);

const edtToolExists$ = bindValue<boolean>(mod.id, "EdtExists");
const transformGizmoToolExists$ = bindValue<boolean>(mod.id, "TransformGizmoToolExists");


function OnEntityHover(index: number, version: number) {
    trigger(mod.id, "OnEntityHover", index, version);
}

function OnEntityLeave(index: number, version: number) {
    trigger(mod.id, "OnEntityLeave", index, version);
}

function OnEntitySelect(index: number, version: number) {

    trigger(mod.id, "OnEntitySelect", index, version);

    const edtToolExists = useValue(edtToolExists$);
    const transformGizmoToolExists = useValue(transformGizmoToolExists$);

    // if (edtToolExists && transformGizmoToolExists) {
    //     trigger("EDT", "TransformGizmoTool.SelectTransformGizmosTool");
    // }
}

function RefreshSelection() {
    trigger(mod.id, "RefreshSelection");
}

function CloseSelectionListPanel() {
    trigger(mod.id, "OnChangeListPanelVisibility");
}

function SelectMarqueeTool() {
    trigger(mod.id, "SelectMarqueeTool");
}

function TogglePanelSize() {
    trigger(mod.id, "OnTogglePanelSize")
}

function SavePanelPosition() {
    const panel = document.getElementById("lsdSelectionListPanel");

    if (!panel) {
        return;
    }

    const rect = panel.getBoundingClientRect();

    const viewportWidth =
        document.documentElement.clientWidth;

    const viewportHeight =
        document.documentElement.clientHeight;

    const x =
        rect.left /
        (viewportWidth - rect.width);

    const y =
        rect.top /
        (viewportHeight - rect.height);

    trigger(
        mod.id,
        "SetPanelPosition",
        { x, y }
    );
}


export const SelectionListPanel = () => {
    const { translate } = useLocalization();

    const listPanelTitle = translate(
        "LAYERED_SELECTION_DISPLAY_LISTPANEL.Title",
        locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.Title"]
    );

    const listPanelRefreshButtonToolTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.RefreshButtonToolTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.RefreshButtonToolTip"]
        ) ?? "Refresh";

    const listPanelMarqueeSelectionToolTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.MarqueeSelectionToolTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.MarqueeSelectionToolTip"]
        ) ?? "Refresh";    

    const removeFromListToolTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.RemoveButtonToolTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.RemoveButtonToolTip"]
        ) ?? "Refresh";

    const listPanelNoItemsSelectedNoMoveIt =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedNoMoveIt",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedNoMoveIt"]
        ) ?? "No items selected";

    const listPanelNoItemsSelectedNoMoveItTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedNoMoveItTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedNoMoveItTip"]
        );

    const listPanelNoItemsSelected =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelected",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelected"]
        ) ?? "No items selected";

    const listPanelNoItemsSelectedTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedTip"]
        );

    const listPanelCloseButtonToolTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.CloseButtonToolTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.CloseButtonToolTip"]
        ) ?? "Close Panel";

    const listPanelExpandButtonToolTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.ExpandButtonToolTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.ExpandButtonToolTip"]
        ) ?? "Expand";

    const listPanelCollapseButtonToolTip =
        translate(
            "LAYERED_SELECTION_DISPLAY_LISTPANEL.CollapseButtonToolTip",
            locale["LAYERED_SELECTION_DISPLAY_LISTPANEL.CollapseButtonToolTip"]
        ) ?? "Collapse";

    const isGame = useValue(isGame$);

    const isMoveItInstalled = useValue(isMoveItInstalled$);

    const isMarqueeToolSelected =
        useValue(isMarqueeToolSelected$);

    const isMarqueeToolActive =
        useValue(isMarqueeToolActive$);

    const marqueeToolIconSrc = isMarqueeToolActive ? marqueeToolActiveSrc : marqueeToolSrc;

    const noItemsTip = isMoveItInstalled ? listPanelNoItemsSelectedTip : listPanelNoItemsSelectedNoMoveItTip;

    const noItemsText = isMoveItInstalled ? listPanelNoItemsSelected : listPanelNoItemsSelectedNoMoveIt;

    const selectedEntities =
        useValue(selectedEntities$);

    const panelPosition =
        useValue(panelPosition$);

    const expandedListPanel = useValue(expandedListPanel$);

    const panelHeight = expandedListPanel ? 640 : 340;

    /*
     * Entities are hidden only from this UI list.
     * They are not removed from the actual Move It of future custom selection.
     */
    const [removedEntityIndexes, setRemovedEntityIndexes] =
        useState<number[]>([]);  

    const visibleEntities =
        [...selectedEntities.Entities]
            .filter(
                (entity: SelectedEntity) =>
                    !removedEntityIndexes.includes(entity.Index)
            )
            .sort(
                (a: SelectedEntity, b: SelectedEntity) =>
                    a.Name.localeCompare(b.Name)
            );

    function HandleRefresh() {
        setRemovedEntityIndexes([]);
        RefreshSelection();
    }

    function HandleRemoveEntity(index: number) {
        setRemovedEntityIndexes(
            previous =>
                previous.includes(index)
                    ? previous
                    : [...previous, index]
        );
    }

    const panelHeader = (
        <div className={`${ styles.panelHeader} header_SUX header_H_U header_Bpo child-opacity-transition_nkS`}>

            <div
                className={styles.dragGrip}
                aria-hidden="true"
            >
                <span />
                <span />
                <span />
            </div>
            <div
                className={styles.dragGrip}
                aria-hidden="true"
            >
                <span />
                <span />
                <span />
            </div>

            <span className={styles.title}>
                {listPanelTitle}
            </span>

            <div className={styles.headerButtons}>

                {/* Marquee */}                
                <Tooltip tooltip={listPanelMarqueeSelectionToolTip} >
                    <button
                        className={styles.headerButton}
                        title={listPanelMarqueeSelectionToolTip}
                        onMouseDown={event =>
                            event.stopPropagation()
                        }
                        onClick={ SelectMarqueeTool }
                            >       
                        <img src={ marqueeToolIconSrc } />
                    </button>
                </Tooltip>
                <>
                    {isMoveItInstalled ? (
                        <>
                            {/* Refresh */}
                            <Tooltip tooltip={listPanelRefreshButtonToolTip} >
                                <button
                                    className={styles.headerButton}
                                    title={listPanelRefreshButtonToolTip}
                                    onMouseDown={event =>
                                        event.stopPropagation()
                                    }
                                    onClick={HandleRefresh}
                                >                                        
                                    <svg
                                        className={
                                            styles.headerIcon
                                        }
                                        width="18"
                                        height="18"
                                        viewBox="0 0 18 18"
                                        fill="none"
                                        aria-hidden="true"
                                    >
                                        <path
                                            d="M14.5 9a5.5 5.5 0 1 1-1.6-3.9"
                                            stroke="currentColor"
                                            strokeWidth="1.4"
                                            strokeLinecap="round"
                                        />

                                        <polyline
                                            points="12.5,2.5 12.5,5.5 9.5,5.5"
                                            stroke="currentColor"
                                            strokeWidth="1.4"
                                            strokeLinecap="round"
                                            strokeLinejoin="round"
                                            fill="none"
                                        />
                                    </svg>
                                </button>
                            </Tooltip>    
                        </>                
                    ) : null }
                </>

                {/* Close */}
                <Tooltip tooltip={listPanelCloseButtonToolTip} >
                    <button
                        className="button_bvQ button_bvQ close-button_wKK"
                        title="Close Panel"
                        onMouseDown={event =>
                            event.stopPropagation()
                        }
                        onClick={CloseSelectionListPanel}
                    >
                        <div className="tinted-icon_iKo icon_PhD" style={{ maskImage: `url(Media/Glyphs/Close.svg)` }}> </div>
                    </button>
                </Tooltip>
            </div>
        </div>
    );

    const panelFooter = (

        <Tooltip tooltip={expandedListPanel ? listPanelCollapseButtonToolTip : listPanelExpandButtonToolTip} >
            <div
                className={`${styles.footer} ${
                    expandedListPanel ? styles.expanded : ""
                } footer_IlY footer_Pa9 footer_pD5 child-opacity-transition_nkS`}
                onMouseDown={event => event.stopPropagation()}
                onClick={() => TogglePanelSize()}
            >
                <span className={styles.footerToggle} />
            </div>
        </Tooltip>

    );

    return (
        <>
            {isGame &&
                isMarqueeToolSelected && (
                    <Portal>

                        <Panel
                            id="lsdSelectionListPanel"
                            draggable
                            initialPosition={panelPosition}
                            className={`${styles.mainPanel} content_AD7 child-opacity-transition_nkS`}
                            style={{
                                height: `${panelHeight}rem`
                            }}
                            header={panelHeader}
                            footer={panelFooter}
                            onMouseUp={SavePanelPosition}
                        >

                            <div className={styles.body}>

                                {visibleEntities.length === 0 ? (

                                    <div className={styles.emptyState}>

                                        <div className={styles.emptyIcon}>
                                            <svg
                                                width="20"
                                                height="20"
                                                viewBox="0 0 20 20"
                                                fill="none"
                                                aria-hidden="true"
                                            >

                                                <rect
                                                    x="3"
                                                    y="3"
                                                    width="14"
                                                    height="14"
                                                    rx="2"
                                                    stroke="currentColor"
                                                    strokeWidth="1.2"
                                                    strokeDasharray="3 2"
                                                />

                                                <path
                                                    d="M7 10h6M10 7v6"
                                                    stroke="currentColor"
                                                    strokeWidth="1.1"
                                                    strokeLinecap="round"
                                                />

                                            </svg>
                                        </div>

                                        <p>
                                            {noItemsText}
                                        </p>

                                    </div>

                                ) : (

                                    <>

                                        <div
                                            className={styles.selectionCount}
                                        >
                                            <p>
                                                <span>
                                                    {visibleEntities.length}
                                                </span>
                                                {" "} elements in selection
                                            </p>
                                        </div>


                                        <Scrollable
                                            className={styles.scrollablePanel}
                                        >

                                            <ul className={styles.listedAssets}>

                                                {visibleEntities.map(
                                                    (entity, index) => (

                                                        <li
                                                            key={entity.Index}
                                                            onMouseDown={() =>
                                                                OnEntitySelect(
                                                                    entity.Index,
                                                                    entity.Version
                                                                )
                                                            }
                                                            onMouseEnter={() =>
                                                                OnEntityHover(
                                                                    entity.Index,
                                                                    entity.Version
                                                                )
                                                            }
                                                            onMouseLeave={() =>
                                                                OnEntityLeave(
                                                                    entity.Index,
                                                                    entity.Version
                                                                )
                                                            }
                                                        >

                                                            <span
                                                                className={
                                                                    styles.entityIndex
                                                                }
                                                            >
                                                                {String(
                                                                    index + 1
                                                                ).padStart(
                                                                    3,
                                                                    "0"
                                                                )}
                                                            </span>

                                                            <span
                                                                className={ styles.separator}
                                                            />

                                                            <span
                                                                className={styles.entityName}
                                                            >
                                                                {entity.Name}
                                                            </span>

                                                            <Tooltip tooltip={removeFromListToolTip} >
                                                                <button
                                                                    className={
                                                                        styles.removeButton
                                                                    }
                                                                    title={removeFromListToolTip}
                                                                    onMouseDown={event =>
                                                                        event.stopPropagation()
                                                                    }
                                                                    onClick={event => {
                                                                        event.stopPropagation();

                                                                        HandleRemoveEntity(
                                                                            entity.Index
                                                                        );
                                                                    }}
                                                                >
                                                                    −
                                                                </button>
                                                            </Tooltip>

                                                        </li>
                                                    )
                                                )}

                                            </ul>

                                        </Scrollable>

                                    </>
                                )}

                            </div>                            

                        </Panel>

                    </Portal>
                )}
        </>
    );
};