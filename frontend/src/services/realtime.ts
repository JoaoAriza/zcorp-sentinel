import * as signalR from "@microsoft/signalr";

export function createIncidentConnection(onIncidentCreated: () => void) {
  const token = localStorage.getItem("zcorp_token");

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5283/hubs/incidents", {
      accessTokenFactory: () => token ?? "",
    })
    .withAutomaticReconnect()
    .build();

  connection.on("incident-created", () => {
    onIncidentCreated();
  });

  return connection;
}