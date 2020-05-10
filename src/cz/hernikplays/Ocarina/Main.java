package cz.hernikplays.Ocarina;

import javafx.application.Application;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.stage.Stage;
import net.arikia.dev.drpc.DiscordRPC;

public class Main extends Application {

    @Override
    public void start(Stage primaryStage) throws Exception{
        FXMLLoader loader = new FXMLLoader(getClass().getResource("mainwindow.fxml"));
        Parent root = loader.load();
        primaryStage.setTitle("Ocarina");
        primaryStage.setScene(new Scene(root, 600, 400));
        primaryStage.setMinWidth(700);
        primaryStage.setMinHeight(500);
        primaryStage.show();

        Controller controller = loader.getController();
        controller.setStage(primaryStage);
    }

    @Override
    public void stop() throws Exception {
        DiscordRPC.discordShutdown();
        super.stop();
    }

    public static void main(String[] args) {
        launch(args);
    }
}
