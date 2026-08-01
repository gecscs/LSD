import { useLocalization } from "cs2/l10n";
import { Panel, Portal, Scrollable,  } from "cs2/ui";
import { bindValue, trigger, useValue } from "cs2/api";
import locale from "../mods/lang/en-US.json";
import styles from "./selectionListPanel.module.scss";
import mod from "../../mod.json";
import { Entity } from "cs2/bindings";
import { SelectedEntities } from "../Domain/SelectedEntities";

const isGame$ = bindValue<boolean>(mod.id, 'IsGame');
const isMarqueeToolSelected$ = bindValue<boolean>(mod.id, "IsMarqueeToolSelected");
const selectedEntities$ = bindValue<SelectedEntities>(mod.id, "SelectedEntities", { Entities: [] });

export const SelectionListPanel = () => {
    const { translate } = useLocalization();
    const listPanelTitle =          translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Title",            locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Title"]);
    const listPanelIntro = translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Intro", locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Intro"]);

    const isGame = useValue(isGame$);
    const isMarqueeToolSelected = useValue(isMarqueeToolSelected$);
    const selectedEntities = useValue(selectedEntities$);

   return (
        <>
        {(isGame && isMarqueeToolSelected) && (
            <Portal>
                <Panel 
                    className={styles.mainPanel}
                    header={listPanelTitle}
                >
                  <div>{listPanelIntro}</div>
                  <Scrollable className={styles.scrollablePanel}>
                    <ul className={styles.listedAssets}>
                        {selectedEntities.Entities.map((e) => (
                            <li key={e.Index}>{e.Index + " : " + e.Version}</li>
                        ))}
                    </ul>
                  </Scrollable>
                </Panel>
            </Portal>
             
        )}
        </>
    );
}
                        