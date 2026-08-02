import { useLocalization } from "cs2/l10n";
import { Panel, Portal, Scrollable } from "cs2/ui";
import { bindValue, trigger, useValue } from "cs2/api";
import locale from "../mods/lang/en-US.json";
import styles from "./selectionListPanel.module.scss";
import mod from "../../mod.json";
import { Entity } from "cs2/bindings";
import { SelectedEntities } from "../Domain/SelectedEntities";
import { SelectedEntity } from "../Domain/SelectedEntity";

const isGame$ = bindValue<boolean>(mod.id, 'IsGame');
const isMarqueeToolSelected$ = bindValue<boolean>(mod.id, "IsMarqueeToolSelected");
const selectedEntities$ = bindValue<SelectedEntities>(mod.id, "SelectedEntities", { Entities: [] });

function OnEntityHover(index: number, version: number) {
    trigger(mod.id, "OnEntityHover", index, version);
}

function OnEntityLeave(index: number, version: number) {
    trigger(mod.id, "OnEntityLeave", index, version);
}    

function OnEntitySelect(index: number, version: number) {
    trigger(mod.id, "OnEntitySelect", index, version);
}   

function RefreshSelection() {
    trigger(mod.id, "RefreshSelection");
}

export const SelectionListPanel = () => {
    const { translate } = useLocalization();
    const listPanelTitle =          translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Title",            locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Title"]);
    const listPanelIntro = translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Intro", locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Intro"]);
    const listPanelRefreshButtonToolTip = translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.RefreshButtonToolTip", locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.RefreshButtonToolTip"]) ?? "Test";
    const listPanelNoItemsSelected = translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelected", locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelected"]) ?? "No items selected";
    const listPanelNoItemsSelectedTip = translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedTip", locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.NoItemsSelectedTip"]);
    const refreshIconSrc = "coui://uil/Standard/Reset.svg"; //RefreshIconSrc;

    const isGame = useValue(isGame$);
    const isMarqueeToolSelected = useValue(isMarqueeToolSelected$);
    const selectedEntities = useValue(selectedEntities$);

    console.debug("SelectionListPanel: selectedEntities.Entities.length = ", selectedEntities.Entities.length);
    console.debug("SelectionListPanel: selectedEntities.Entities = ", selectedEntities.Entities ?? "No entities available");

    selectedEntities.Entities.sort((a: SelectedEntity, b: SelectedEntity) => a.Name.localeCompare(b.Name));

    console.debug("SelectionListPanel: selectedEntities.Entities after sorting = ", selectedEntities.Entities ?? "No entities available");

   return (
        <>
        {(isGame && isMarqueeToolSelected) && (
            <Portal>
                <Panel 
                    className={styles.mainPanel}
                    header={listPanelTitle}
                >
                    <div className={ styles.introBar }>
                        <div className={ styles.introText }>
                            { listPanelIntro }
                        </div>

                        <button
                            
                            className={ styles.refreshButton }
                            onClick={() => RefreshSelection()}
                            title={ listPanelRefreshButtonToolTip }
                        >
                            <img src={ refreshIconSrc }/>
                         </button>
                    </div>    
                    {selectedEntities.Entities.length > 0 && (
                        <Scrollable className={styles.scrollablePanel}>
                            <ul className={styles.listedAssets}>
                                {selectedEntities.Entities.sort((a: SelectedEntity, b: SelectedEntity) => a.Name.localeCompare(b.Name)).map((e) => (
                                    <li 
                                        onMouseDown={() => OnEntitySelect(e.Index, e.Version)}
                                        onMouseEnter={() => OnEntityHover(e.Index, e.Version)}
                                        onMouseLeave={() => OnEntityLeave(e.Index, e.Version)}
                                        key={e.Index} > {e.Name} </li>
                                ))}
                            </ul>
                        </Scrollable>
                    )}
                    {selectedEntities.Entities.length === 0 && (
                        <div className={styles.noItemsSelected}>
                            <span>{ listPanelNoItemsSelected } </span>
                            <p> { listPanelNoItemsSelectedTip } </p>
                        </div>
                    )}  
                </Panel>
            </Portal>
             
        )}
        </>
    );
}
                        