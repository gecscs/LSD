import { useLocalization } from "cs2/l10n";
import { Panel, Portal, Scrollable,  } from "cs2/ui";
import { bindValue, trigger, useValue } from "cs2/api";
import locale from "../mods/lang/en-US.json";
import styles from "./myComponent.module.scss";
import mod from "../../mod.json";

const isGame$ = bindValue<boolean>(mod.id, 'IsGame');
const isMarqueeToolActive$ = bindValue<boolean>(mod.id, "IsMarqueeToolActive", false);    

export const MyComponent = () => {
    const { translate } = useLocalization();
    const listPanelTitle =          translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Title",            locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Title"]);
    const listPanelIntro = translate("LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Intro", locale["LSD_LAYERED_SELECTION_DISPLAY_LISTPANEL.Intro"]);

    const isGame = useValue(isGame$);
    const isMarqueeToolActive = useValue(isMarqueeToolActive$);

   return (
        <>
        {(isGame && isMarqueeToolActive) && (
            <Portal>
                <Panel 
                    className={styles.mainPanel}
                    header={listPanelTitle}
                >
                  <div>{listPanelIntro}</div>
                  <Scrollable className={styles.scrollablePanel}>
                    <ul className={styles.listedAssets}>
                        <li>Item 1</li>
                        <li>Item 2</li>
                        <li>Item 3</li>
                        <li>Item 4</li>
                        <li>Item 5</li>
                        <li>Item 6</li>
                        <li>Item 7</li>
                        <li>Item 8</li>
                        <li>Item 9</li>
                        <li>Item 1</li>
                        <li>Item 11</li>
                        <li>Item 12</li>
                        <li>Item 13</li>
                        <li>Item 14</li>
                        <li>Item 15</li>
                        <li>Item 16</li>
                        <li>Item 17</li>
                        <li>Item 18</li>
                        <li>Item 19</li>
                        <li>Item 20</li>
                        <li>Item 21</li>
                        <li>Item 22</li>
                        <li>Item 23</li>
                        <li>Item 24</li>
                    </ul>
                    
                  </Scrollable>
                </Panel>
            </Portal>
             
        )}
        </>
    );
}